import 'twitter'

twitter = None
twitter_name = None
lastCheck = date.fromtimestamp(time.time());
playlist = list()

def initApis:
	twitter = twitter.Api(consumer_key='consumer_key',
                      consumer_secret='consumer_secret',
                      access_token_key='access_token',
                      access_token_secret='access_token_secret')
	twitter_name = twitter.VerifyCredentials()["name"]
    
def checkTwitter:                  
	if twitter != None:
		status_temp = twitter.GetStatus()
		status = list()
		for stat in status_temp:
			if stat.GetCreatedAtInSeconds() >= lastCheck and stat.GetUser() != twitter_name:
				status.append(stat)
		for stat in status:
			if "https://www.youtube.com" in stat.GetText():
				playlist.append(re.match("(https://www.youtube.com/.*) ").group(0))
			elif "https://youtu.be" in stat.GetText():
				playlist.append(re.match("(https://youtu.be/.*)").group(0))
			elif "https://soundcloud.com" in stat.GetText():
				playlist.append(re.match("(https://soundcloud.com/.*/.*) ").group(0))
			elif "https://m.soundcloud.com" in stat.GetText():
				playlist.append(re.match("(https://m.soundcloud.com/.*/.*) ").group(0))
		status_temp = date.fromtimestamp(time.time());
				
			
			
