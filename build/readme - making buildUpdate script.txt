(Re)building buildUpdate.sh requires ruby and https://github.com/chrisvire/BuildUpdate
Here's the command line commands I used:

cd <path to where you want to generate buildUpdate.sh>
<your path to buildupdate.rb (part of BuildUpdate repo above)>\buildupdate.rb -t bt431 -f buildUpdate.sh -r ..

Explanation:

"-t bt431" points at the configuration that tracks this branch
"-f ____" gives what I want the file to be called
"-r .." takes care of moving the context up from build to the root directory