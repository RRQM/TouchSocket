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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket;

internal sealed class PackageCodeBuilder : CodeBuilder
{
    private readonly SourceProductionContext m_context;
    private readonly string m_packageBaseString = "TouchSocket.Core.PackageBase";
    private readonly INamedTypeSymbol m_packageClass;
    private readonly string m_packageMemberAttributeString = "TouchSocket.Core.PackageMemberAttribute";

    public PackageCodeBuilder(INamedTypeSymbol packageClass, SourceProductionContext context)
    {
        this.m_packageClass = packageClass;
        this.m_context = context;
    }

    public override string Id => this.m_packageClass.ToDisplayString();
    public string Prefix { get; set; }

    public override string GetFileName()
    {
        return $"{this.m_packageClass.Name}_Package.g.cs";
    }

    #region Converter

    private void BuildConverter(StringBuilder codeBuilder, IEnumerable<PackageMember> members)
    {
        foreach (var item in members)
        {
            var converter = item.Converter;
            if (converter != null)
            {
                codeBuilder.AppendLine($"private static readonly {converter.ToDisplayString()} {this.GetConverterVariableName(converter)} = new {converter.ToDisplayString()}();");
            }
        }
    }

    private string GetConverterVariableName(ITypeSymbol typeSymbol)
    {
        return $"m_{typeSymbol.Name}";
    }

    #endregion Converter

    #region Package

    private int m_deep;

    private void AppendArrayWriteString(StringBuilder codeBuilder, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
    {
        var arrayTypeSymbol = (IArrayTypeSymbol)typeSymbol;
        if (!this.SupportType(arrayTypeSymbol.ElementType))
        {
            this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, name, arrayTypeSymbol));
            return;
        }

        codeBuilder.AppendLine($"WriterExtension.WriteIsNull(ref writer,{name});");
        codeBuilder.AppendLine($"if ({name}!=null)");
        codeBuilder.AppendLine("{");

        var rank = arrayTypeSymbol.Rank;
        if (rank == 1)
        {
            codeBuilder.AppendLine($"WriterExtension.WriteVarUInt32(ref writer,(uint){name}.Length);");
        }
        else
        {
            var dimensionItem = this.GetDeepItemString();
            codeBuilder.AppendLine($"for (var {dimensionItem} = 0; {dimensionItem} < {rank}; {dimensionItem}++)");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"WriterExtension.WriteVarUInt32(ref writer,(uint){name}.GetLength({dimensionItem}));");
            codeBuilder.AppendLine("}");
        }

        var item = this.GetDeepItemString();
        codeBuilder.AppendLine($"foreach (var {item} in {name})");
        codeBuilder.AppendLine("{");
        this.AppendObjectWriteString(codeBuilder, packageMember, arrayTypeSymbol.ElementType, item);
        codeBuilder.AppendLine("}");
        codeBuilder.AppendLine("}");
    }

    private void AppendObjectWriteString(StringBuilder codeBuilder, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
    {
        //Debugger.Launch();
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            //枚举
            this.AppendWriteString(codeBuilder, packageMember, typeSymbol, name);
        }
        else if (this.CanReadWrite(typeSymbol))
        {
            //直接读写
            this.AppendWriteString(codeBuilder, packageMember, typeSymbol, name);
        }
        else if (packageMember.Converter != null)
        {
            //转换器
            codeBuilder.AppendLine($"{this.GetConverterVariableName(packageMember.Converter)}.Package(ref writer,this.{packageMember.Name});");
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
            codeBuilder.AppendLine($"WriterExtension.WriteIsNull(ref writer,{name});");
            codeBuilder.AppendLine($"if ({name}!=null)");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"WriterExtension.WriteVarUInt32(ref writer,(uint){name}.Count);");
            var item = this.GetDeepItemString();
            codeBuilder.AppendLine($"foreach (var {item} in {name})");
            codeBuilder.AppendLine("{");
            this.AppendObjectWriteString(codeBuilder, packageMember, elementType, item);
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("}");
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
            codeBuilder.AppendLine($"WriterExtension.WriteIsNull(ref writer,{name});");
            codeBuilder.AppendLine($"if ({name}!=null)");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"WriterExtension.WriteVarUInt32(ref writer,(uint){name}.Count);");

            var item = this.GetDeepItemString();

            codeBuilder.AppendLine($"foreach (var {item} in {name})");
            codeBuilder.AppendLine("{");
            this.AppendObjectWriteString(codeBuilder, packageMember, elementTypeKey, $"{item}.Key");
            this.AppendObjectWriteString(codeBuilder, packageMember, elementTypeValue, $"{item}.Value");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("}");
        }
        else if (typeSymbol.TypeKind == TypeKind.Class)
        {
            if (!typeSymbol.IsInheritFrom(Utils.IPackageTypeName))
            {
                this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, packageMember.Name, packageMember.Type));
                return;
            }
            this.AppendWriteString(codeBuilder, packageMember, typeSymbol, name);
        }
        else if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                var typeSymbolNotNullable = typeSymbol.GetNullableType();
                if (!typeSymbolNotNullable.IsInheritFrom(Utils.IPackageTypeName))
                {
                    this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, packageMember.Name, packageMember.Type));
                    return;
                }
            }
            else
            {
                if (!typeSymbol.IsInheritFrom(Utils.IPackageTypeName))
                {
                    this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0002, packageMember.Location, packageMember.Name, packageMember.Type));
                    return;
                }
            }
            this.AppendWriteString(codeBuilder, packageMember, typeSymbol, name);
        }
        else if (typeSymbol.TypeKind == TypeKind.Array)
        {
            this.AppendArrayWriteString(codeBuilder, packageMember, typeSymbol, name);
        }
    }

    private void AppendWriteString(StringBuilder codeBuilder, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
    {
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            var namedTypeSymbol = (INamedTypeSymbol)typeSymbol;
            var enumUnderlyingType = namedTypeSymbol.EnumUnderlyingType;

            var writeString = this.GetWriteString(enumUnderlyingType, $"({enumUnderlyingType.ToDisplayString()}){name}");
            codeBuilder.AppendLine($"{writeString};");
        }
        else if (this.CanReadWrite(typeSymbol))
        {
            codeBuilder.AppendLine($"{this.GetWriteString(typeSymbol, name)};");
        }
        else if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            if (typeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
            {
                codeBuilder.AppendLine($"{name}.Package(ref writer);");
            }
            else
            {
                codeBuilder.AppendLine($"if ({name}.HasValue)");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine($" WriterExtension.WriteNotNull(ref writer);");
                codeBuilder.AppendLine($"this.{name}.Value.Package(ref writer);");
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine($"else");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine("WriterExtension.WriteNull(ref writer);");
                codeBuilder.AppendLine("}");
            }
        }
        else if (typeSymbol.TypeKind == TypeKind.Array)
        {
            this.AppendArrayWriteString(codeBuilder, packageMember, typeSymbol, name);
        }
        else
        {
            codeBuilder.AppendLine($"if ({name}!=null)");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($" WriterExtension.WriteNotNull(ref writer);");
            codeBuilder.AppendLine($"{name}.Package(ref writer);");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine($"else");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("WriterExtension.WriteNull(ref writer);");
            codeBuilder.AppendLine("}");
        }
    }

    private void BuildPackage(StringBuilder codeBuilder, IEnumerable<PackageMember> members)
    {
        if (this.ExistsPackageMethod())
        {
            return;
        }
        //Debugger.Launch();
        var OverrideMethod = this.NeedOverridePackageMethod(this.m_packageClass.BaseType);
        if (OverrideMethod != null)
        {
            codeBuilder.AppendLine($"public override void Package<TWriter>(ref TWriter writer)");
            codeBuilder.AppendLine("{");

            if (OverrideMethod.IsVirtual || this.IsGeneratorPackage(this.m_packageClass.BaseType))
            {
                codeBuilder.AppendLine("base.Package(ref writer);");
            }
        }
        else
        {
            codeBuilder.AppendLine($"public void Package<TWriter>(ref TWriter writer) where TWriter: IBytesWriter");
            codeBuilder.AppendLine("{");
        }
        foreach (var packageMember in members)
        {
            this.AppendObjectWriteString(codeBuilder, packageMember, packageMember.Type, packageMember.Name);
        }
        codeBuilder.AppendLine("}");
    }

    private bool CanReadWrite(ITypeSymbol typeSymbol)
    {
        //Debugger.Launch();
        var typeSymbolNotNullable = typeSymbol.GetNullableType();
        if (typeSymbolNotNullable.IsInheritFrom(Utils.IPackageTypeName))
        {
            return false;
        }

        if (typeSymbol.IsUnmanagedType())
        {
            return true;
        }
        if (typeSymbol.SpecialType == SpecialType.System_String)
        {
            return true;
        }

        return false;
    }

    private int GetDeep()
    {
        return this.m_deep++;
    }

    private string GetDeepItemString()
    {
        return "item" + this.GetDeep();
    }

    private string GetWriteString(ITypeSymbol typeSymbol, string name)
    {
        if (typeSymbol.SpecialType == SpecialType.System_String)
        {
            return $"WriterExtension.WriteString(ref writer,{name})";
        }
        return $"WriterExtension.WriteValue<TWriter,{typeSymbol.ToDisplayString()}>(ref writer,{name})";
    }

    private bool IsGeneratorPackage(INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol == null)
        {
            return false;
        }
        return namedTypeSymbol.HasAttribute(Utils.GeneratorPackageAttributeTypeName);
    }

    #endregion Package

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

        if (typeSymbol.IsInheritFrom(Utils.IPackageTypeName))
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

    private void AppendArrayReadString(StringBuilder codeBuilder, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
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
            codeBuilder.AppendLine("if (!ReaderExtension.ReadIsNull(ref reader))");
            codeBuilder.AppendLine("{");
            var len = this.GetDeepItemString();
            codeBuilder.AppendLine($"var {len}=(int)ReaderExtension.ReadVarUInt32(ref reader);");
            codeBuilder.AppendLine($"{name} = new {elementType.ToDisplayString()}[{len}];");

            var i = this.GetDeepItemString();
            codeBuilder.AppendLine($"for (var {i} = 0; {i} < {len}; {i}++)");
            codeBuilder.AppendLine("{");
            if (this.CanReadWrite(elementType))
            {
                codeBuilder.AppendLine($"{name}[{i}] = {this.GetReadString(elementType)};");
            }
            else
            {
                var item = this.GetDeepItemString();
                codeBuilder.AppendLine($"{elementType.ToDisplayString()} {item}=default;");
                this.AppendObjectReadString(codeBuilder, packageMember, elementType, item);
                codeBuilder.AppendLine($"{name}[{i}] = {item};");
            }
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("}");
        }
        else
        {
            codeBuilder.AppendLine("if (!ReaderExtension.ReadIsNull(ref reader))");
            codeBuilder.AppendLine("{");

            var dimensionNames = new string[rank];
            for (var j = 0; j < rank; j++)
            {
                var lenName = this.GetDeepItemString();
                codeBuilder.AppendLine($"var {lenName}=(int)ReaderExtension.ReadVarUInt32(ref reader);");
                dimensionNames[j] = lenName;
            }

            var array = this.GetDeepItemString();

            codeBuilder.Append($"var {array} = new {elementType.ToDisplayString()}[{string.Join(",", dimensionNames)}];");

            this.AppendDimensionArrayReadString(codeBuilder, dimensionNames, 0, elementType, array, packageMember, new string[rank]);

            codeBuilder.Append($"{name} = {array};");
            codeBuilder.AppendLine("}");
        }
    }

    private void AppendDimensionArrayReadString(StringBuilder codeBuilder, string[] dimensionNames, int dimensionNameIndex, ITypeSymbol elementType, string name, PackageMember packageMember, string[] dimensionIndexNames)
    {
        var i = this.GetDeepItemString();
        dimensionIndexNames[dimensionNameIndex] = i;
        var len = dimensionNames[dimensionNameIndex];
        codeBuilder.AppendLine($"for (var {i} = 0; {i} < {len}; {i}++)");
        codeBuilder.AppendLine("{");
        if (dimensionNameIndex == dimensionNames.Length - 1)
        {
            //最后一个维度
            if (this.CanReadWrite(elementType))
            {
                codeBuilder.AppendLine($"{name}[{string.Join(",", dimensionIndexNames)}] = {this.GetReadString(elementType)};");
            }
            else
            {
                var item = this.GetDeepItemString();
                codeBuilder.AppendLine($"{elementType.ToDisplayString()} {item}=default;");
                this.AppendObjectReadString(codeBuilder, packageMember, elementType, item);
                codeBuilder.AppendLine($"{name}[{string.Join(",", dimensionIndexNames)}] = {item};");
            }
        }
        else
        {
            this.AppendDimensionArrayReadString(codeBuilder, dimensionNames, dimensionNameIndex + 1, elementType, name, packageMember, dimensionIndexNames);
        }

        codeBuilder.AppendLine("}");
    }

    private void AppendObjectReadString(StringBuilder codeBuilder, PackageMember packageMember, ITypeSymbol typeSymbol, string name)
    {
        //Debugger.Launch();
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            //枚举
            codeBuilder.AppendLine($"{name}=({typeSymbol.ToDisplayString()}){this.GetReadString(((INamedTypeSymbol)typeSymbol).EnumUnderlyingType)};");
        }
        else if (this.CanReadWrite(typeSymbol))
        {
            //直接读写
            codeBuilder.AppendLine($"{name}={this.GetReadString(typeSymbol)};");
        }
        else if (packageMember.Converter != null)
        {
            //转换器
            codeBuilder.AppendLine($"this.{packageMember.Name}=({typeSymbol.ToDisplayString()}){this.GetConverterVariableName(packageMember.Converter)}.Unpackage(ref reader);");
        }
        else if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsList())
        {
            //list
            var elementType = namedTypeSymbol.TypeArguments[0];
            if (!this.SupportType(elementType))
            {
                return;
            }
            codeBuilder.AppendLine("if (!ReaderExtension.ReadIsNull(ref reader))");
            codeBuilder.AppendLine("{");
            var len = this.GetDeepItemString();
            codeBuilder.AppendLine($"var {len}=(int)ReaderExtension.ReadVarUInt32(ref reader);");

            var list = this.GetDeepItemString();
            codeBuilder.AppendLine($"var {list} = new System.Collections.Generic.List<{elementType.ToDisplayString()}>({len});");

            var i = this.GetDeepItemString();

            codeBuilder.AppendLine($"for (var {i} = 0; {i} < {len}; {i}++)");
            codeBuilder.AppendLine("{");
            if (this.CanReadWrite(elementType))
            {
                codeBuilder.AppendLine($"{list}.Add({this.GetReadString(elementType)});");
            }
            else
            {
                var itemName = this.GetDeepItemString();
                codeBuilder.AppendLine($"{elementType.ToDisplayString()} {itemName}=default;");
                this.AppendObjectReadString(codeBuilder, packageMember, elementType, itemName);

                codeBuilder.AppendLine($"{list}.Add({itemName});");
            }

            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine($"{name}={list};");
            codeBuilder.AppendLine("}");
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
            codeBuilder.AppendLine("if (!ReaderExtension.ReadIsNull(ref reader))");
            codeBuilder.AppendLine("{");

            var len = this.GetDeepItemString();
            codeBuilder.AppendLine($"var {len}=(int)ReaderExtension.ReadVarUInt32(ref reader);");

            var dic = this.GetDeepItemString();
            codeBuilder.AppendLine($"var {dic} = new System.Collections.Generic.Dictionary<{elementTypeKey.ToDisplayString()},{elementTypeValue.ToDisplayString()}>({len});");

            var i = this.GetDeepItemString();
            codeBuilder.AppendLine($"for (var {i} = 0; {i} < {len}; {i}++)");
            codeBuilder.AppendLine("{");

            var key = this.GetDeepItemString();
            if (this.CanReadWrite(elementTypeKey))
            {
                codeBuilder.AppendLine($"var {key} = {this.GetReadString(elementTypeKey)};");
            }
            else
            {
                codeBuilder.AppendLine($"{elementTypeKey.ToDisplayString()} {key}=default;");
                this.AppendObjectReadString(codeBuilder, packageMember, elementTypeKey, key);
            }

            var value = this.GetDeepItemString();
            if (this.CanReadWrite(elementTypeValue))
            {
                codeBuilder.AppendLine($"var {value} = {this.GetReadString(elementTypeValue)};");
            }
            else
            {
                codeBuilder.AppendLine($"{elementTypeValue.ToDisplayString()} {value}=default;");
                this.AppendObjectReadString(codeBuilder, packageMember, elementTypeValue, value);
            }
            codeBuilder.AppendLine($"{dic}.Add({key},{value});");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine($"{name}={dic};");
            codeBuilder.AppendLine("}");
        }
        else if (typeSymbol.TypeKind == TypeKind.Class)
        {
            if (!typeSymbol.IsInheritFrom(Utils.IPackageTypeName))
            {
                return;
            }
            codeBuilder.AppendLine($"if(!ReaderExtension.ReadIsNull(ref reader))");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine($"{name}=new {typeSymbol.ToDisplayString()}();");
            codeBuilder.AppendLine($"{name}.Unpackage(ref reader);");
            codeBuilder.AppendLine("}");
        }
        else if (typeSymbol.TypeKind == TypeKind.Struct)
        {
            var propertySymbolName = this.GetDeepItemString();

            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                if (!typeSymbol.GetNullableType().IsInheritFrom(Utils.IPackageTypeName))
                {
                    return;
                }

                codeBuilder.AppendLine($"if(!ReaderExtension.ReadIsNull(ref reader))");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine($"var {propertySymbolName}=new {typeSymbol.GetNullableType().ToDisplayString()}();");
                codeBuilder.AppendLine($"{propertySymbolName}.Unpackage(ref reader);");
                codeBuilder.AppendLine($"this.{name} = {propertySymbolName};");
                codeBuilder.AppendLine("}");
            }
            else
            {
                if (!typeSymbol.IsInheritFrom(Utils.IPackageTypeName))
                {
                    return;
                }

                codeBuilder.AppendLine($"var {propertySymbolName}=new {typeSymbol.ToDisplayString()}();");
                codeBuilder.AppendLine($"{propertySymbolName}.Unpackage(ref reader);");
                codeBuilder.AppendLine($"{name}={propertySymbolName};");
            }
        }
        else if (typeSymbol.TypeKind == TypeKind.Array)
        {
            this.AppendArrayReadString(codeBuilder, packageMember, typeSymbol, name);
        }
    }

    private void BuildUnpackage(StringBuilder codeBuilder, IEnumerable<PackageMember> members)
    {
        if (this.ExistsUnpackageMethod())
        {
            return;
        }

        var OverrideMethod = this.NeedOverrideUnpackageMethod(this.m_packageClass.BaseType);
        if (OverrideMethod != null)
        {
            codeBuilder.AppendLine($"public override void Unpackage<TReader>(ref TReader reader)");
            codeBuilder.AppendLine("{");
            if (OverrideMethod.IsVirtual || this.IsGeneratorPackage(this.m_packageClass.BaseType))
            {
                codeBuilder.AppendLine("base.Unpackage(ref reader);");
            }
        }
        else
        {
            codeBuilder.AppendLine($"public void Unpackage<TReader>(ref TReader reader) where TReader : IBytesReader");
            codeBuilder.AppendLine("{");
        }

        foreach (var packageMember in members)
        {
            this.AppendObjectReadString(codeBuilder, packageMember, packageMember.Type, packageMember.Name);
        }
        codeBuilder.AppendLine("}");
    }

    private string GetReadString(ITypeSymbol typeSymbol)
    {
        if (typeSymbol.SpecialType == SpecialType.System_String)
        {
            return $"ReaderExtension.ReadString(ref reader)";
        }
        return $"ReaderExtension.ReadValue<TReader,{typeSymbol.ToDisplayString()}>(ref reader)";
    }

    #endregion Unpackage

    #region override

    protected override bool GeneratorCode(StringBuilder codeBuilder)
    {
        if (this.m_packageClass.IsInheritFrom(this.m_packageBaseString))
        {
        }
        else if (this.m_packageClass.TypeKind == TypeKind.Struct)
        {
        }
        else
        {
            this.m_context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.m_rule_Package0001, this.m_packageClass.Locations[0]));
            return false;
        }

        //Debugger.Launch();
        if (!this.m_packageClass.ContainingNamespace.IsGlobalNamespace)
        {
            codeBuilder.AppendLine($"namespace {this.m_packageClass.ContainingNamespace}");
            codeBuilder.AppendLine("{");
        }

        codeBuilder.AppendLine($"partial {(this.m_packageClass.TypeKind == TypeKind.Struct ? "struct" : "class")} {this.m_packageClass.Name}");
        codeBuilder.AppendLine("{");

        var members = this.GetPackageMembers();

        this.BuildConverter(codeBuilder, members);

        this.m_deep = 0;
        this.BuildPackage(codeBuilder, members);

        this.m_deep = 0;
        this.BuildUnpackage(codeBuilder, members);
        codeBuilder.AppendLine("}");

        if (!this.m_packageClass.ContainingNamespace.IsGlobalNamespace)
        {
            codeBuilder.AppendLine("}");
        }
        // System.Diagnostics.Debugger.Launch();
        return true;
    }

    private bool ExistsPackageMethod()
    {
        return this.m_packageClass
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m =>
            {
                if (m.Name == "Package" && m.Parameters.Length == 1)
                {
                    if (m.Parameters[0].RefKind == RefKind.Ref && m.Parameters[0].Type.ToDisplayString() == "TWriter")
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
                    if (m.Parameters[0].RefKind == RefKind.Ref && m.Parameters[0].Type.ToDisplayString() == "TReader")
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
                     if (m.Parameters[0].RefKind == RefKind.Ref && m.Parameters[0].Type.ToDisplayString() == "TWriter")
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
                     if (m.Parameters[0].RefKind == RefKind.Ref && m.Parameters[0].Type.ToDisplayString() == "TReader")
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
        public ITypeSymbol Converter { get; set; }
        public int Index { get; set; }
        public Location Location { get; set; }
        public string Name { get; set; }
        public ITypeSymbol Type { get; set; }
    }

    #endregion Class
}