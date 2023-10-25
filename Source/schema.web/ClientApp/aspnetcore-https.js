// This script sets up HTTPS for the application using the ASP.NET Core HTTPS certificate
const fs = require('fs');
const spawn = require('child_process').spawn;
const path = require('path');

const baseFolder = "./certs" ||
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
let certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;

// if (!certificateName) {
//   console.error('Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.')
//   process.exit(-1);
// }
certificateName =certificateName || "schema.client";
const certFilePath = path.join(__dirname,"certs", `schema.client.crt`);
const keyFilePath = path.join(__dirname,"certs", `schema.client.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  spawn('dotnet', [
    'dev-certs',
    'https',
    '--export-path',
    certFilePath,
    '--format',
    'Pem',
    '--no-password'

  ], { stdio: 'inherit', })
  .on('exit', (code) => process.exit(code));
}
