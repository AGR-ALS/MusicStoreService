# Setup

## Clone Repository
```
git clone https://github.com/AGR-ALS/MusicStoreService.git
cd MusicStoreService/
```

## Enter enviromental variables

Create `.env` files and write variables into them according to `.env_examples` files. 

For example:
```
DeepLTranslationSettings__ApiKey="your_api_key"
```

## Launch the App
### On Windows
```
docker build -t music-service . ; docker run -p 8080:1544 -d --name music-service music-service
```
### On Linux
```
docker build -t music-service . && docker run -p 8080:1544 -d --name music-service music-service
```

### You can access the app at [localhost:8080](http://localhost:8080)
