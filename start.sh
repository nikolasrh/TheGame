dotnet build

mintty bash -c "dotnet run --project TheGame.GameServer; read -p \"Press enter to close the window...\""
sleep 3
mintty bash -c "dotnet run --project TheGame.GameClient; read -p \"Press enter to close the window...\""
mintty bash -c "dotnet run --project TheGame.GameClient; read -p \"Press enter to close the window...\""
