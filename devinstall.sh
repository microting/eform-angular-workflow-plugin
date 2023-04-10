#!/bin/bash
cd ~
pwd

rm -fR Documents/workspace/microting/eform-angular-frontend/eform-client/src/app/plugins/modules/workflow-pn

cp -a Documents/workspace/microting/eform-angular-workflow-plugin/eform-client/src/app/plugins/modules/workflow-pn Documents/workspace/microting/eform-angular-frontend/eform-client/src/app/plugins/modules/workflow-pn

mkdir Documents/workspace/microting/eform-angular-frontend/eFormAPI/Plugins

rm -fR Documents/workspace/microting/eform-angular-frontend/eFormAPI/Plugins/Workflow.Pn

cp -a Documents/workspace/microting/eform-angular-workflow-plugin/eFormAPI/Plugins/Workflow.Pn Documents/workspace/microting/eform-angular-frontend/eFormAPI/Plugins/Workflow.Pn

# Test files rm
rm -fR Documents/workspace/microting/eform-angular-frontend/eform-client/e2e/Tests/workflow-settings
rm -fR Documents/workspace/microting/eform-angular-frontend/eform-client/e2e/Tests/workflow-general
rm -fR Documents/workspace/microting/eform-angular-frontend/eform-client/e2e/Page\ objects/Workflow
rm -fR Documents/workspace/microting/eform-angular-frontend/eform-client/wdio-plugin-step2.conf.ts 

# Test files cp
cp -a Documents/workspace/microting/eform-angular-workflow-plugin/eform-client/e2e/Tests/workflow-settings Documents/workspace/microting/eform-angular-frontend/eform-client/e2e/Tests/workflow-settings
cp -a Documents/workspace/microting/eform-angular-workflow-plugin/eform-client/e2e/Tests/workflow-general Documents/workspace/microting/eform-angular-frontend/eform-client/e2e/Tests/workflow-general
cp -a Documents/workspace/microting/eform-angular-workflow-plugin/eform-client/e2e/Page\ objects/Workflow Documents/workspace/microting/eform-angular-frontend/eform-client/e2e/Page\ objects/Workflow
cp -a Documents/workspace/microting/eform-angular-workflow-plugin/eform-client/wdio-headless-plugin-step2.conf.ts  Documents/workspace/microting/eform-angular-frontend/eform-client/wdio-plugin-step2.conf.ts 
