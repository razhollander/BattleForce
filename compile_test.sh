#!/bin/bash
dotnet new console -n TempCompileTest
cp -R Assets TempCompileTest/
cd TempCompileTest
dotnet build --no-restore
cd ..
rm -rf TempCompileTest
