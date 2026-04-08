#!/bin/bash
find ./Assets/Core/Game/Domains/GamePlay -name "*.cs" | grep -v "/Tests/" > filelist.txt
cat filelist.txt | wc -l
