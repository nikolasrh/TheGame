dotnet build

if ! ps -W | grep -q TheGame.GameServer; then
    mintty bash -c "dotnet run --project TheGame.GameServer; read -p \"Press enter to close the window...\""
    sleep 3
fi

mintty bash -c "dotnet run --project TheGame.ConsoleClient; read -p \"Press enter to close the window...\""

mintty bash -i -c "cd TheGame.GodotClient; godot; read -p \"Press enter to close the window...\""
