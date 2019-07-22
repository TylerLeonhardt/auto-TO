# auto-TO
Quick and easy chat bot which, given a mongo database containing a list of names and phone numbers, a Twillio account, and a Challonge account, will run a basic tournament for you.

Names should be ordered into tiers.  The winner should text the Twillio number back saying they won.  The Twilio endpoint should point to where this is hosted, or an ngrok instance if that's preferred.

Today this assumes that the winning player 2-0'd the loser.  We'd love to improve this to allow for better keeping track, but that's where we are today.