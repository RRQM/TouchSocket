const { processFile } = require('./replace-definitions-precise.js');

const filePath = 'd:/CodeOpen/TouchSocket/handbook/docs/dmtpclient.mdx';
const result = processFile(filePath);

console.log('处理结果：', result);
