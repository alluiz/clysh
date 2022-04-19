#!/bin/bash

now=$(date +"%Y%m%d_%H%M%S")

dotnet-coverage collect "dotnet test" --output-format "cobertura" --output "./temp/${now}_coverage" --settings ./test.runsettings -id "${uuid}"

#HTML
reportgenerator -reports:"./temp/${now}_coverage.cobertura.xml" -targetdir:"./coverage/report/html" -reporttypes:"Html" -historydir:"./coverage/history"

#BADGE
reportgenerator -reports:"./temp/${now}_coverage.cobertura.xml" -targetdir:"./coverage/report/badge" -reporttypes:"Badges" -historydir:"./coverage/history"

rm -rf "./temp"