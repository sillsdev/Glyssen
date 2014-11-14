Protoscript Generator
====================
A joint project between Faith Comes by Hearing and SIL International, Protoscript Generator is a tool for parsing Scripture passages and assigning characters who speak each section.  It allows a native speaker to disambiguate blocks of text which may be spoken by more than one character.

We're currently looking for a better name.

Source Code
====================
Protoscript Generator is written in C#. The UI uses Windows Forms.

Getting up-to-date libraries
====================
We depend on several libraries, notably libpalaso from SIL International and L10NSharp.
The source contains a script for downloading dependencies from TeamCity.

From the build directory, run the buildUpdate.sh script:

	cd {ProtoscriptGeneratorRoot}/build
	./buildUpdate.sh

To automatically generate buildUpdate.sh from a setup on TeamCity, see
	
	 {ProtoscriptGeneratorRoot}/build/readme - making buildUpdate script.txt

Platform
====================
At least initially, Protoscript Generator is only on Windows.