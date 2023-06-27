# BackStone
Backend repo for capstone project

# Setup
We are using environment variables for the yelp API Key. To pass the key into the container while debugging in vs, create a backstone/settings.env file with YelpApiKey={your yelp api key}

### Start
docker-compose build --no-cache
docker-compose up -d

### Stop
docker-compose down

### Remove all images, containers, and volumes
docker system prune -a

# Testing
In order to run the integration tests you will need an API Key. Add the API key into the setup method of BackstoneRepositoryTests before running the tests or set it locally on your machine.

## Running server locally and connecting via xcode
Backend project needs to be running first...
- Create an account with ngrok
- Download and follow instructions for setting ngrok up
- Navigate to ngrok folder and run ./ngrok http 55094
- Verify https://{ngrok public url}/swagger contains the swagger page
- Copy and paste the ngrok public url into the development.xcodeconfig 

