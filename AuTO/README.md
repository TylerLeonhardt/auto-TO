# How to run this:

Run AuTO from Visual Studio 2017.
Run this command:
  ngrok hhtp 7071
ngrok will spit out an api endpoint.  Put that in Twilio.
Any text to AuTO will now function properly, assuming the database works...

# Updating the database:

Use Robo 3T with the connection string in local.settings.json to edit the database.
Participants need a new challonge id, and first timers need to be added to the participant list.

# Steps:

Add new players with number, name, etc in mongo db
Use exact names matching mongodb in challonge
Send master text to running AuTO instance
Tell the first n people to start playing and text "i won" when they won
Enjoy the tournament!
