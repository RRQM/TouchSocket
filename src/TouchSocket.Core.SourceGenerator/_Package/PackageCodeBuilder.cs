//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket;

internal sealed class PackageCodeBuilder : CodeBuilder
{
    private readonly string m_byteBlockString = "TByteBlock";

    private readonly GeneratorExecutionContext m_context;
    private readonly string m_packageBaseString = "TouchSocket.Core.PackageBase";
    private readonly string m_packageMemberAttributeString = "TouchSocket.Core.PackageMemberAttribute";

    private readonly INamedTypeSymbol m_packageClass;

    public PackageCodeBuilder(INamedTypeSymbol packageClass, GeneratorExecutionContext context)
    {
        this.m_packageClass = packageClass;
        this.m_context = context;
    }

    public override string Id => this.m_packageClass.ToDisplayString();
    public string Prefix { get; set; }

    public IEnumerable<string> Usings
    {
        get
        {
            yield return "using System;";
            yield return "using System.Diagnostics;";
            yield return "using TouchSocket.Core;";
            yield return "using System.Threading.Tasks;";
        }
    }

    public override string GetFileName()
    {
        return this.m_packageClass.ToDisplayString() + "Generator";
    }

    public override string ToString()
    {
        var codeString = new StringBuilder();
        codeString.AppendLine("/*");
        codeString.AppendLine("此代码由SourceGenerator工具直接生成，非必要请不要修改此处代码");
        codeString.AppendLine("*/");
        codeString.AppendLine("#pragma warning disable");

        foreach (var item in this.Usings)
        {
            codeString.AppendLine(item);
        }

        //Debugger.Launch();
        if (!this.m_packageClass.ContainingNamespace.IsGlobalNamespace)
        {
            codeString.AppendLine($"namespace {this.m_packageClass.ContainingNamespace}");
            codeString.AppendLine("{");
        }


        codeString.AppendLine($"partial {(this.m_packageClass.TypeKind == TypeKind.Struct ? "struct" : "class")} {this.m_packageClass.Name}");
        codeString.AppendLine("{");

        var members = this.GetPackageMembers();

        this.BuildConverter(codeString, members);

        this.m_deep = 0;
        this.BuildPackage(codeString, members);

        this.m_deep = 0;
        this.BuildUnpackage(codeString, members);
        codeString.AppendLine("}");

        if (!this.m_packageClass.ContainingNamespace.IsGlobalNamespace)
        {
            codeString.AppendLine("}");
        }
        // System.Diagnostics.Debugger.Launch();
        return codeString.ToString();
    }

    #region Converter
    private void BuildConverter(StringBuilder codeString, IEnumerable<PackageMember> members)
    {
        foreach (var item in members)
        {
            var converter = item.Converter;
            if (converter != null)
            {
                codeString.AppendLine($"private static readonly IFastBinaryConverter {this.GetConverterVariableName(converter)} = new {converter.ToDisplayString()}();");
            }
        }
    }

    private string GetConverterVariableName(ITypeSymbol typeSymbol)
    {
        return $"m_{typeSymbol.Name}";
    }
    #endregion

    #region Package

    private int m_deep;

    private void AppendArrayWriteString(StringBuilder codeString, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
    {
        var arrayTypeSymbol = (IArrayTypeSymbol)typeSymbol;
        if (!this.SupportType(arrayTypeSymbol.ElementType))
        {
            this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, name, arrayTypeSymbol));
            return;
        }

        codeString.AppendLine($"byteBlock.WriteIsNull({name});");
        codeString.AppendLine($"if ({name}!=null)");
        codeString.AppendLine("{");

        var rank = arrayTypeSymbol.Rank;
        if (rank == 1)
        {
            codeString.AppendLine($"byteBlock.WriteVarUInt32((uint){name}.Length);");
        }
        else
        {
            var dimensionItem = this.GetDeepItemString();
            codeString.AppendLine($"for (var {dimensionItem} = 0; {dimensionItem} < {rank}; {dimensionItem}++)");
            codeString.AppendLine("{");
            codeString.AppendLine($"byteBlock.WriteVarUInt32((uint){name}.GetLength({dimensionItem}));");
            codeString.AppendLine("}");
        }

        var item = this.GetDeepItemString();
        codeString.AppendLine($"foreach (var {item} in {name})");
        codeString.AppendLine("{");
        this.AppendObjectWriteString(codeString, packageMember, arrayTypeSymbol.ElementType, item);
        codeString.AppendLine("}");
        codeString.AppendLine("}");
    }

    private void AppendObjectWriteString(StringBuilder codeString, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
    {
        //Debugger.Launch();
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            //枚举
            this.AppendWriteString(codeString, packageMember, typeSymbol, name);
        }
        else if (this.CanReadWrite(typeSymbol))
        {
            //直接读写
            this.AppendWriteString(codeString, packageMember, typeSymbol, name);
        }
        else if (packageMember.Converter != null)
        {
            //转换器
            codeString.AppendLine($"{this.GetConverterVariableName(packageMember.Converter)}.Write(ref byteBlock,this.{packageMember.Name});");
        }
        else if (typeSymbol is INamedTypeSymbol listNamedTypeSymbol && listNamedTypeSymbol.IsList())
        {
            //List
            //Debugger.Launch();
            var elementType = listNamedTypeSymbol.TypeArguments[0];
            if (!this.SupportType(elementType))
            {
                this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, packageMember.Name, elementType));
                return;
            }
            codeString.AppendLine($"byteBlock.WriteIsNull({name});");
            codeString.AppendLine($"if ({name}!=null)");
            codeString.AppendLine("{");
            codeString.AppendLine($"byteBlock.WriteVarUInt32((uint){name}.Count);");
            var item = this.GetDeepItemString();
            codeString.AppendLine($"foreach (var {item} in {name})");
            codeString.AppendLine("{");
            this.AppendObjectWriteString(codeString, packageMember, elementType, item);
            codeString.AppendLine("}");
            codeString.AppendLine("}");
        }
        else if (typeSymbol is INamedTypeSymbol dicNamedTypeSymbol && dicNamedTypeSymbol.IsDictionary())
        {
            //Dictionary

            var elementTypeKey = dicNamedTypeSymbol.TypeArguments[0];
            var elementTypeValue = dicNamedTypeSymbol.TypeArguments[1];
            if (!this.SupportType(elementTypeKey))
            {
                this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, packageMember.Name, elementTypeKey));
                return;
            }
            if (!this.SupportType(elementTypeValue))
            {
                this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, packageMember.Name, elementTypeValue));
                return;
            }
            codeString.AppendLine($"byteBlock.WriteIsNull({name});");
            codeString.AppendLine($"if ({name}!=null)");
            codeString.AppendLine("{");
            codeString.AppendLine($"byteBlock.WriteVarUInt32((uint){name}.Count);");

            var item = this.GetDeepItemString();

            codeString.AppendLine($"foreach (var {item} in {name})");
            codeString.AppendLine("{");
            this.AppendObjectWriteString(codeString, packageMember, elementTypeKey, $"{item}.Key");
            this.AppendObjectWriteString(codeString, packageMember, elementTypeValue, $"{item}.Value");
            codeString.AppendLine("}");
            codeString.AppendLine("}");
        }
        else if (typeSymbol.TypeKind == TypeKind.Class)
        {
            if (!typeSymbol.IsInheritFrom(PackageSyntaxReceiver.IPackageTypeName))
            {
                this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, packageMember.Name, packageMember.Type));
                return;
            }
            this.AppendWriteString(codeString, packageMember, typeSymbol, name);
        }
        else if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                var typeSymbolNotNullable = typeSymbol.GetNullableType();
                if (!typeSymbolNotNullable.WithNullableAnnotation(NullableAnnotation.None).IsInheritFrom(PackageSyntaxReceiver.IPackageTypeName))
                {
                    this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, packageMember.Name, packageMember.Type));
                    return;
                }
            }
            else
            {
                if (!typeSymbol.IsInheritFrom(PackageSyntaxReceiver.IPackageTypeName))
                {
                    this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, packageMember.Name, packageMember.Type));
                    return;
                }
            }
            this.AppendWriteString(codeString, packageMember, typeSymbol, name);
        }
        else if (typeSymbol.TypeKind == TypeKind.Array)
        {
            this.AppendArrayWriteString(codeString, packageMember, typeSymbol, name);
        }
    }

    private void AppendWriteString(StringBuilder codeString, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
    {
        //Debugger.Launch();
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            var namedTypeSymbol = (INamedTypeSymbol)typeSymbol;
            var enumUnderlyingType = namedTypeSymbol.EnumUnderlyingType;

            var writeString = this.GetWriteString(enumUnderlyingType, $"({enumUnderlyingType.ToDisplayString()}){name}");
            codeString.AppendLine($"{writeString};");
        }
        else if (this.CanReadWrite(typeSymbol))
        {
            codeString.AppendLine($"{this.GetWriteString(typeSymbol, name)};");
        }
        else if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            if (typeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                codeString.AppendLine($"{name}.Package(ref byteBlock);");
            }
            else
            {
                codeString.AppendLine($"if ({name}.HasValue)");
                codeString.AppendLine("{");
                codeString.AppendLine($"byteBlock.WriteNotNull();");
                codeString.AppendLine($"this.{name}.Value.Package(ref byteBlock);");
                codeString.AppendLine("}");
                codeString.AppendLine($"else");
                codeString.AppendLine("{");
                codeString.AppendLine("byteBlock.WriteNull();");
                codeString.AppendLine("}");
            }
        }
        else if (typeSymbol.TypeKind == TypeKind.Array)
        {
            this.AppendArrayWriteString(codeString, packageMember, typeSymbol, name);
        }
        else
        {
            codeString.AppendLine($"byteBlock.WritePackage({name});");
        }
    }

    private string GetWriteString(ITypeSymbol typeSymbol, string name)
    {
        if (typeSymbol.SpecialType == SpecialType.System_Boolean)
        {
            return $"byteBlock.WriteBoolean({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Byte)
        {
            return $"byteBlock.WriteByte({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_SByte)
        {
            return $"byteBlock.WriteInt16({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Int16)
        {
            return $"byteBlock.WriteInt16({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_UInt16)
        {
            return $"byteBlock.WriteUInt16({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Int32)
        {
            return $"byteBlock.WriteInt32({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_UInt32)
        {
            return $"byteBlock.WriteUInt32({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Int64)
        {
            return $"byteBlock.WriteInt64({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_UInt64)
        {
            return $"byteBlock.WriteUInt64({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Single)
        {
            return $"byteBlock.WriteFloat({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Double)
        {
            return $"byteBlock.WriteDouble({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_String)
        {
            return $"byteBlock.WriteString({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Char)
        {
            return $"byteBlock.WriteChar({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Decimal)
        {
            return $"byteBlock.WriteDecimal({name})";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_DateTime)
        {
            return $"byteBlock.WriteDateTime({name})";
        }
        else if (typeSymbol.IsTimeSpan())
        {
            return $"byteBlock.WriteTimeSpan({name})";
        }
        else if (typeSymbol.IsGuid())
        {
            return $"byteBlock.WriteGuid({name})";
        }
        else if (typeSymbol.ToDisplayString() == "byte[]")
        {
            return $"byteBlock.WriteBytesPackage({name})";
        }
        else
        {
            return "";
        }
    }

    private void BuildPackage(StringBuilder codeString, IEnumerable<PackageMember> members)
    {
        if (this.ExistsPackageMethod())
        {
            return;
        }
        //Debugger.Launch();
        var OverrideMethod = this.NeedOverridePackageMethod(this.m_packageClass.BaseType);
        if (OverrideMethod != null)
        {
            codeString.AppendLine($"public override void Package<TByteBlock>(ref TByteBlock byteBlock)");
            codeString.AppendLine("{");

            if (OverrideMethod.IsVirtual || this.IsGeneratorPackage(this.m_packageClass.BaseType))
            {
                codeString.AppendLine("base.Package(ref byteBlock);");
            }
        }
        else
        {
            codeString.AppendLine($"public void Package<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock:IByteBlock");
            codeString.AppendLine("{");
        }
        foreach (var packageMember in members)
        {
            this.AppendObjectWriteString(codeString, packageMember, packageMember.Type, packageMember.Name);
        }
        codeString.AppendLine("}");
    }

    private bool IsGeneratorPackage(INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol == null)
        {
            return false;
        }
        return namedTypeSymbol.HasAttribute(PackageSyntaxReceiver.GeneratorPackageAttributeTypeName);
    }

    private bool CanReadWrite(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.IsTimeSpan())
        {
            return true;
        }
        if (typeSymbol.IsGuid())
        {
            return true;
        }

        if (typeSymbol.ToDisplayString() == "byte[]")
        {
            return true;
        }
        switch (typeSymbol.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Char:
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Decimal:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_String:
            case SpecialType.System_DateTime:
                {
                    return true;
                }
            default:
                return false;
        }
    }

    private int GetDeep()
    {
        return this.m_deep++;
    }

    private string GetDeepItemString()
    {
        return "item" + this.GetDeep();
    }

    #endregion Package

    public bool TryToSourceText(out SourceText sourceText)
    {
        var b = false;
        if (this.m_packageClass.IsInheritFrom(this.m_packageBaseString))
        {
            b = true;
        }
        else if (this.m_packageClass.TypeKind == TypeKind.Struct)
        {
            b = true;
        }
        else
        {
            this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0001, this.m_packageClass.Locations[0]));
        }
        if (b)
        {
            var code = this.ToString();
            if (string.IsNullOrEmpty(code))
            {
                sourceText = null;
                return false;
            }
            sourceText = SourceText.From(code, Encoding.UTF8);
            return true;
        }
        sourceText = null;
        return false;
    }

    private IEnumerable<PackageMember> GetPackageMembers()
    {
        var list = new List<PackageMember>();

        foreach (var item in this.m_packageClass.GetMembers().OfType<IPropertySymbol>())
        {
            if (item.IsReadOnly)
            {
                continue;
            }

            if (item.IsOverride)
            {
                continue;
            }

            var packageMember = new PackageMember()
            {
                Name = item.Name,
                Type = item.Type,
                Location = item.Locations[0]
            };
            if (item.HasAttribute(this.m_packageMemberAttributeString, out var attributeData))
            {
                var namedDic = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);

                if (namedDic.TryGetValue(nameof(PackageMemberAttribute.Behavior), out var typedConstant))
                {
                    var behavior = (PackageBehavior)typedConstant.Value;
                    if (behavior == PackageBehavior.Ignore)
                    {
                        //忽略
                        continue;
                    }
                }

                if (namedDic.TryGetValue(nameof(PackageMemberAttribute.Index), out typedConstant))
                {
                    var index = (int)typedConstant.Value;
                    packageMember.Index = index;
                }

                if (namedDic.TryGetValue(nameof(PackageMemberAttribute.Converter), out typedConstant))
                {
                    var converter = (ITypeSymbol)typedConstant.Value;
                    packageMember.Converter = converter;
                }
            }

            list.Add(packageMember);
        }

        foreach (var item in this.m_packageClass.GetMembers().OfType<IFieldSymbol>())
        {
            if (item.IsReadOnly)
            {
                continue;
            }

            if (item.IsOverride)
            {
                continue;
            }

            var packageMember = new PackageMember()
            {
                Name = item.Name,
                Type = item.Type,
                Location = item.Locations[0]
            };
            if (item.HasAttribute(this.m_packageMemberAttributeString, out var attributeData))
            {
                var namedDic = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);

                if (namedDic.TryGetValue(nameof(PackageMemberAttribute.Behavior), out var typedConstant))
                {
                    var behavior = (PackageBehavior)typedConstant.Value;
                    if (behavior != PackageBehavior.Include)
                    {
                        //忽略
                        continue;
                    }
                }

                if (namedDic.TryGetValue(nameof(PackageMemberAttribute.Index), out typedConstant))
                {
                    var index = (int)typedConstant.Value;
                    packageMember.Index = index;
                }

                if (namedDic.TryGetValue(nameof(PackageMemberAttribute.Converter), out typedConstant))
                {
                    var converter = (ITypeSymbol)typedConstant.Value;
                    packageMember.Converter = converter;
                }
            }
            else
            {
                continue;
            }

            list.Add(packageMember);
        }

        list.Sort((a, b) =>
        {
            if (a.Index > b.Index)
            {
                return 1;
            }

            return 0;
        });

        return list;
    }

    private bool SupportType(ITypeSymbol typeSymbol)
    {
        if (this.CanReadWrite(typeSymbol))
        {
            return true;
        }

        if (typeSymbol.IsInheritFrom(PackageSyntaxReceiver.IPackageTypeName))
        {
            return true;
        }

        if (typeSymbol is IArrayTypeSymbol arraySymbol)
        {
            return this.SupportType(arraySymbol.ElementType);
        }

        if (typeSymbol is INamedTypeSymbol listNamedTypeSymbol && listNamedTypeSymbol.IsList())
        {
            return this.SupportType(listNamedTypeSymbol.TypeArguments[0]);
        }

        return false;
    }

    #region Unpackage

    private void AppendArrayReadString(StringBuilder codeString, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
    {
        var arrayTypeSymbol = (IArrayTypeSymbol)typeSymbol;
        var elementType = arrayTypeSymbol.ElementType;

        if (!this.SupportType(elementType))
        {
            return;
        }

        var rank = arrayTypeSymbol.Rank;

        if (rank == 1)
        {
            codeString.AppendLine("if (!byteBlock.ReadIsNull())");
            codeString.AppendLine("{");
            var len = this.GetDeepItemString();
            codeString.AppendLine($"var {len}=(int)byteBlock.ReadVarUInt32();");
            codeString.AppendLine($"{name} = new {elementType.ToDisplayString()}[{len}];");

            var i = this.GetDeepItemString();
            codeString.AppendLine($"for (var {i} = 0; {i} < {len}; {i}++)");
            codeString.AppendLine("{");
            if (this.CanReadWrite(elementType))
            {
                codeString.AppendLine($"{name}[{i}] = {this.GetReadString(elementType)};");
            }
            else
            {
                var item = this.GetDeepItemString();
                codeString.AppendLine($"{elementType.ToDisplayString()} {item}=default;");
                this.AppendObjectReadString(codeString, packageMember, elementType, item);
                codeString.AppendLine($"{name}[{i}] = {item};");
            }
            codeString.AppendLine("}");
            codeString.AppendLine("}");
        }
        else
        {
            codeString.AppendLine("if (!byteBlock.ReadIsNull())");
            codeString.AppendLine("{");

            var dimensionNames = new string[rank];
            for (var j = 0; j < rank; j++)
            {
                var lenName = this.GetDeepItemString();
                codeString.AppendLine($"var {lenName}=(int)byteBlock.ReadVarUInt32();");
                dimensionNames[j] = lenName;
            }

            var array = this.GetDeepItemString();

            codeString.Append($"var {array} = new {elementType.ToDisplayString()}[{string.Join(",", dimensionNames)}];");


            this.AppendDimensionArrayReadString(codeString, dimensionNames, 0, elementType, array, packageMember, new string[rank]);

            codeString.Append($"{name} = {array};");
            codeString.AppendLine("}");
        }
    }

    private void AppendDimensionArrayReadString(StringBuilder codeString, string[] dimensionNames, int dimensionNameIndex, ITypeSymbol elementType, string name, PackageMember packageMember, string[] dimensionIndexNames)
    {
        var i = this.GetDeepItemString();
        dimensionIndexNames[dimensionNameIndex] = i;
        var len = dimensionNames[dimensionNameIndex];
        codeString.AppendLine($"for (var {i} = 0; {i} < {len}; {i}++)");
        codeString.AppendLine("{");
        if (dimensionNameIndex == dimensionNames.Length - 1)
        {
            //最后一个维度
            if (this.CanReadWrite(elementType))
            {
                codeString.AppendLine($"{name}[{string.Join(",", dimensionIndexNames)}] = {this.GetReadString(elementType)};");
            }
            else
            {
                var item = this.GetDeepItemString();
                codeString.AppendLine($"{elementType.ToDisplayString()} {item}=default;");
                this.AppendObjectReadString(codeString, packageMember, elementType, item);
                codeString.AppendLine($"{name}[{string.Join(",", dimensionIndexNames)}] = {item};");
            }
        }
        else
        {
            this.AppendDimensionArrayReadString(codeString, dimensionNames, dimensionNameIndex + 1, elementType, name, packageMember, dimensionIndexNames);
        }

        codeString.AppendLine("}");
    }

    private void AppendObjectReadString(StringBuilder codeString, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
    {
        //Debugger.Launch();
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            //枚举
            codeString.AppendLine($"{name}=({typeSymbol.ToDisplayString()}){this.GetReadString(((INamedTypeSymbol)typeSymbol).EnumUnderlyingType)};");
        }
        else if (this.CanReadWrite(typeSymbol))
        {
            //直接读写
            codeString.AppendLine($"{name}={this.GetReadString(typeSymbol)};");
        }
        else if (packageMember.Converter != null)
        {
            //转换器
            codeString.AppendLine($"this.{packageMember.Name}=({typeSymbol.ToDisplayString()}){this.GetConverterVariableName(packageMember.Converter)}.Read(ref byteBlock,typeof({typeSymbol.ToDisplayString()}));");
        }
        else if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsList())
        {
            //list
            var elementType = namedTypeSymbol.TypeArguments[0];
            if (!this.SupportType(elementType))
            {
                return;
            }
            codeString.AppendLine("if (!byteBlock.ReadIsNull())");
            codeString.AppendLine("{");
            var len = this.GetDeepItemString();
            codeString.AppendLine($"var {len}=(int)byteBlock.ReadVarUInt32();");

            var list = this.GetDeepItemString();
            codeString.AppendLine($"var {list} = new System.Collections.Generic.List<{elementType.ToDisplayString()}>({len});");

            var i = this.GetDeepItemString();

            codeString.AppendLine($"for (var {i} = 0; {i} < {len}; {i}++)");
            codeString.AppendLine("{");
            if (this.CanReadWrite(elementType))
            {
                codeString.AppendLine($"{list}.Add({this.GetReadString(elementType)});");
            }
            else
            {
                var itemName = this.GetDeepItemString();
                codeString.AppendLine($"{elementType.ToDisplayString()} {itemName}=default;");
                this.AppendObjectReadString(codeString, packageMember, elementType, itemName);

                codeString.AppendLine($"{list}.Add({itemName});");
            }

            codeString.AppendLine("}");
            codeString.AppendLine($"{name}={list};");
            codeString.AppendLine("}");
        }
        else if (typeSymbol is INamedTypeSymbol dicNamedTypeSymbol && dicNamedTypeSymbol.IsDictionary())
        {
            //Dictionary
            var elementTypeKey = dicNamedTypeSymbol.TypeArguments[0];
            var elementTypeValue = dicNamedTypeSymbol.TypeArguments[1];
            if (!this.SupportType(elementTypeKey))
            {
                return;
            }
            if (!this.SupportType(elementTypeValue))
            {
                return;
            }
            codeString.AppendLine("if (!byteBlock.ReadIsNull())");
            codeString.AppendLine("{");

            var len = this.GetDeepItemString();
            codeString.AppendLine($"var {len}=(int)byteBlock.ReadVarUInt32();");

            var dic = this.GetDeepItemString();
            codeString.AppendLine($"var {dic} = new System.Collections.Generic.Dictionary<{elementTypeKey.ToDisplayString()},{elementTypeValue.ToDisplayString()}>({len});");

            var i = this.GetDeepItemString();
            codeString.AppendLine($"for (var {i} = 0; {i} < {len}; {i}++)");
            codeString.AppendLine("{");

            var key = this.GetDeepItemString();
            if (this.CanReadWrite(elementTypeKey))
            {
                codeString.AppendLine($"var {key} = {this.GetReadString(elementTypeKey)};");
            }
            else
            {
                codeString.AppendLine($"{elementTypeKey.ToDisplayString()} {key}=default;");
                this.AppendObjectReadString(codeString, packageMember, elementTypeKey, key);
            }

            var value = this.GetDeepItemString();
            if (this.CanReadWrite(elementTypeValue))
            {
                codeString.AppendLine($"var {value} = {this.GetReadString(elementTypeValue)};");
            }
            else
            {
                codeString.AppendLine($"{elementTypeValue.ToDisplayString()} {value}=default;");
                this.AppendObjectReadString(codeString, packageMember, elementTypeValue, value);
            }
            codeString.AppendLine($"{dic}.Add({key},{value});");
            codeString.AppendLine("}");
            codeString.AppendLine($"{name}={dic};");
            codeString.AppendLine("}");
        }
        else if (typeSymbol.TypeKind == TypeKind.Class)
        {
            if (!typeSymbol.IsInheritFrom(PackageSyntaxReceiver.IPackageTypeName))
            {
                return;
            }
            codeString.AppendLine($"if(!byteBlock.ReadIsNull())");
            codeString.AppendLine("{");
            codeString.AppendLine($"{name}=new {typeSymbol.ToDisplayString()}();");
            codeString.AppendLine($"{name}.Unpackage(ref byteBlock);");
            codeString.AppendLine("}");
        }
        else if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            var propertySymbolName = this.GetDeepItemString();

            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                if (!typeSymbol.GetNullableType().IsInheritFrom(PackageSyntaxReceiver.IPackageTypeName))
                {
                    return;
                }

                codeString.AppendLine($"if(!byteBlock.ReadIsNull())");
                codeString.AppendLine("{");
                codeString.AppendLine($"var {propertySymbolName}=new {typeSymbol.GetNullableType().ToDisplayString()}();");
                codeString.AppendLine($"{propertySymbolName}.Unpackage(ref byteBlock);");
                codeString.AppendLine($"this.{name} = {propertySymbolName};");
                codeString.AppendLine("}");
            }
            else
            {
                if (!typeSymbol.IsInheritFrom(PackageSyntaxReceiver.IPackageTypeName))
                {
                    return;
                }

                codeString.AppendLine($"var {propertySymbolName}=new {typeSymbol.ToDisplayString()}();");
                codeString.AppendLine($"{propertySymbolName}.Unpackage(ref byteBlock);");
                codeString.AppendLine($"{name}={propertySymbolName};");
            }
        }
        else if (typeSymbol.TypeKind == TypeKind.Array)
        {
            this.AppendArrayReadString(codeString, packageMember, typeSymbol, name);
        }
    }

    private void BuildUnpackage(StringBuilder codeString, IEnumerable<PackageMember> members)
    {
        if (this.ExistsUnpackageMethod())
        {
            return;
        }

        var OverrideMethod = this.NeedOverrideUnpackageMethod(this.m_packageClass.BaseType);
        if (OverrideMethod != null)
        {
            codeString.AppendLine($"public override void Unpackage<TByteBlock>(ref TByteBlock byteBlock)");
            codeString.AppendLine("{");
            if (OverrideMethod.IsVirtual || this.IsGeneratorPackage(this.m_packageClass.BaseType))
            {
                codeString.AppendLine("base.Unpackage(ref byteBlock);");
            }
        }
        else
        {
            codeString.AppendLine($"public void Unpackage<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock");
            codeString.AppendLine("{");
        }

        foreach (var packageMember in members)
        {
            this.AppendObjectReadString(codeString, packageMember, packageMember.Type, packageMember.Name);
        }
        codeString.AppendLine("}");
    }

    private string GetReadString(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.SpecialType == SpecialType.System_Boolean)
        {
            return "byteBlock.ReadBoolean()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Byte)
        {
            return "byteBlock.ReadByte()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_SByte)
        {
            return "(sbyte)byteBlock.ReadInt16()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Int16)
        {
            return "byteBlock.ReadInt16()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_UInt16)
        {
            return "byteBlock.ReadUInt16()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Int32)
        {
            return "byteBlock.ReadInt32()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_UInt32)
        {
            return "byteBlock.ReadUInt32()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Int64)
        {
            return "byteBlock.ReadInt64()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_UInt64)
        {
            return "byteBlock.ReadUInt64()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Single)
        {
            return "byteBlock.ReadFloat()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Double)
        {
            return "byteBlock.ReadDouble()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_String)
        {
            return "byteBlock.ReadString()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Char)
        {
            return "byteBlock.ReadChar()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_Decimal)
        {
            return "byteBlock.ReadDecimal()";
        }
        else if (typeSymbol.SpecialType == SpecialType.System_DateTime)
        {
            return "byteBlock.ReadDateTime()";
        }
        else if (typeSymbol.IsTimeSpan())
        {
            return "byteBlock.ReadTimeSpan()";
        }
        else if (typeSymbol.IsGuid())
        {
            return "byteBlock.ReadGuid()";
        }
        else if (typeSymbol.ToDisplayString() == "byte[]")
        {
            return "byteBlock.ReadBytesPackage()";
        }
        else
        {
            return "";
        }
    }

    #endregion Unpackage

    #region override

    private bool ExistsPackageMethod()
    {
        return this.m_packageClass
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m =>
            {
                if (m.Name == "Package" && m.Parameters.Length == 1)
                {
                    if (m.Parameters[0].RefKind == RefKind.Ref && m.Parameters[0].Type.ToDisplayString() == this.m_byteBlockString)
                    {
                        return true;
                    }
                }
                return false;
            });
    }

    private bool ExistsUnpackageMethod()
    {
        return this.m_packageClass
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m =>
            {
                if (m.Name == "Unpackage" && m.Parameters.Length == 1)
                {
                    if (m.Parameters[0].RefKind == RefKind.Ref && m.Parameters[0].Type.ToDisplayString() == this.m_byteBlockString)
                    {
                        return true;
                    }
                }
                return false;
            });
    }

    private IMethodSymbol NeedOverridePackageMethod(INamedTypeSymbol baseTypeSymbol)
    {
        if (baseTypeSymbol == null)
        {
            return default;
        }

        var m = baseTypeSymbol
             .GetMembers()
             .OfType<IMethodSymbol>()
             .FirstOrDefault(m =>
             {
                 //Debugger.Launch();
                 //var ss = m.Parameters[0].Type.ToDisplayString();

                 if (m.Name == "Package" && m.Parameters.Length == 1)
                 {
                     if (!(m.IsAbstract || m.IsVirtual))
                     {
                         return false;
                     }
                     if (m.Parameters[0].RefKind == RefKind.Ref && m.Parameters[0].Type.ToDisplayString() == this.m_byteBlockString)
                     {
                         return true;
                     }
                 }

                 return false;
             });

        if (m != null)
        {
            return m;
        }
        return this.NeedOverridePackageMethod(baseTypeSymbol.BaseType);
    }

    private IMethodSymbol NeedOverrideUnpackageMethod(INamedTypeSymbol baseTypeSymbol)
    {
        if (baseTypeSymbol == null)
        {
            return default;
        }

        var m = baseTypeSymbol
             .GetMembers()
             .OfType<IMethodSymbol>()
             .FirstOrDefault(m =>
             {
                 if (m.Name == "Unpackage" && m.Parameters.Length == 1)
                 {
                     if (!(m.IsAbstract || m.IsVirtual))
                     {
                         return false;
                     }
                     if (m.Parameters[0].RefKind == RefKind.Ref && m.Parameters[0].Type.ToDisplayString() == this.m_byteBlockString)
                     {
                         return true;
                     }
                 }
                 return false;
             });

        if (m != null)
        {
            return m;
        }
        return this.NeedOverrideUnpackageMethod(baseTypeSymbol.BaseType);
    }

    #endregion override

    #region Class
    private class PackageMember
    {
        public int Index { get; set; }
        public ITypeSymbol Converter { get; set; }
        public string Name { get; set; }
        public ITypeSymbol Type { get; set; }
        public Location Location { get; set; }
    }
    #endregion
}