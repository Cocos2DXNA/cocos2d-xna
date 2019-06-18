export ANT_HOME=/Applications/nant-0.92/bin
export ANTHOME=/Applications/nant-0.92/bin
export PATH=/Library/Frameworks/Mono.framework/Home/bin:/Applications/nant-0.92/bin:$PATH
mono $ANT_HOME/NAnt.exe -buildfile:macos.build > macos.out 2>&1
mono $ANT_HOME/NAnt.exe -buildfile:deploy-MacOS.build > deploy.out 2>&1
