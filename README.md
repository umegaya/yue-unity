yue-unity
=========

yue-unity integration lib and sample with NLua installation


usage
=====

for testing [sample project](https://github.com/umegaya/yue-unity/blob/master/sample/Assets/Sample.unity), you need to [docker](https://www.docker.com/) and [yue](https://github.com/umegaya/yue) to be installed, and should run yue server like following
```
cd path_to_yue_repository
./yue sample/unity_test_server.lua
```

then, 

1. Create GameObject which attached [NetworkManager.cs](https://github.com/umegaya/yue-unity/blob/master/sample/Assets/NetworkManager.cs)

2. for calling server RPC, create Actor by using [NetworkManager.NewActor](https://github.com/umegaya/yue-unity/blob/master/sample/Assets/NetworkObject.cs#L17) and  calling method with [Actor.Call](https://github.com/umegaya/yue-unity/blob/master/sample/Assets/NetworkObject.cs#L38)

3. for receiving server notification, register MonoBehavior to NetworkManager by using [NetworkManager.Register](https://github.com/umegaya/yue-unity/blob/master/sample/Assets/NetworkObject.cs#L19) with identifier used in server code.
server notification is represented as RPC from server, like following code
```
local p = luact.peer("/sys") -- calling GameObject which name is "/sys"
print('client call', p:GetUnityVersion(true)) -- calling method of above object which name is GetUnityVersion.
print('client2 call', p:GetUnityVersion(false))
```

full usage is [here](https://github.com/umegaya/yue-unity/tree/master/sample).

