Glyssen
====================
Glyssen makes it easy to produce a high-quality dramatized audio recording of Scripture. It takes the drudgery out of the process by helping to identify all the direct speech in the text and quickly identifying the biblical character who speaks each part. Then it walks you through the process of selecting a cast and assigning roles to the voice actors. Finally, it prepares a complete set of scripts to use in the recording process and helps to ensure that each part is recorded and prepared for post-production.

This is a joint project between [Faith Comes by Hearing](http://www.faithcomesbyhearing.com) and [SIL International](http://www.sil.org).

To learn more about Glyssen, visit [software.sil.org/glyssen](http://software.sil.org/glyssen).

Source Code
====================
Glyssen is written in C# using Windows Forms.

We have a battery of unit tests written using NUnit.

Getting up-to-date libraries
====================
We depend on some libraries built on SIL's TeamCity agents.
The source contains a script for downloading dependencies from TeamCity.

From the build directory, run the buildUpdate.sh script:

	cd {GlyssenRoot}/build
	./buildUpdate.sh

If changes are made to the dependencies on TeamCity, the build update script should be regenerated. To automatically generate buildUpdate.sh from a setup on TeamCity, see
	
	 {GlyssenRoot}/build/readme - making buildUpdate script.txt

Platform
====================
Glyssen only runs on Windows, but FCBH is working on a new cross-platform product that will use the GlyssenEngine.
