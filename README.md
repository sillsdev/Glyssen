Glyssen
====================
A joint project between Faith Comes by Hearing and SIL International, Glyssen is a tool for parsing Scripture passages and assigning characters who speak each section. It allows a native speaker to disambiguate blocks of text which may be spoken by more than one character.

Source Code
====================
Glyssen is written in C#. The UI uses Windows Forms.

Getting up-to-date libraries
====================
We depend on several libraries, notably libpalaso from SIL International and L10NSharp.
The source contains a script for downloading dependencies from TeamCity.

From the build directory, run the buildUpdate.sh script:

	cd {ProtoscriptGeneratorRoot}/build
	./buildUpdate.sh

To automatically generate buildUpdate.sh from a setup on TeamCity, see
	
	 {GlyssenRoot}/build/readme - making buildUpdate script.txt

Platform
====================
At least initially, Glyssen is only on Windows.