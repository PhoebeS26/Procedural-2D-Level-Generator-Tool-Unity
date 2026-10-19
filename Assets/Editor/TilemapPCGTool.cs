using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapPCGTool : EditorWindow
{
  
    // Size presets
    public enum LayoutSize { Small, Medium, Large }

    LayoutSize layoutSize = LayoutSize.Medium;

    int width, height;
    int roomCount, roomMinSize, roomMaxSize;
    int corridorThickness;

    const int roomPadding = 3;

    // Seed
    bool useRandomSeed = true;
    int seed = 0;

    // Corridor variation
    int corridorOffset = 4;

    // Tilemap references
    [SerializeField] Tilemap tilemap;
    [SerializeField] Sprite floorSprite;
    [SerializeField] Sprite wallSprite;
    [SerializeField] Sprite startRoomSprite;
    [SerializeField] Sprite endRoomSprite;

    Color roomFloorColor = Color.white;
    Color corridorColor = new Color(0.6f, 0.5f, 0.4f);

    // Map data
    int[,] map;
    List<RectInt> rooms = new List<RectInt>();
    RectInt startRoom, endRoom;

    string statusMessage = "";
    bool hasGenerated = false;

    [MenuItem("Tools/Tilemap PCG Generator")]
    public static void OpenWindow() => GetWindow<TilemapPCGTool>("Tilemap PCG");

    void OnGUI()
    {
        GUILayout.Label("Level Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        GUILayout.Label("References", EditorStyles.miniBoldLabel);
        tilemap = (Tilemap)EditorGUILayout.ObjectField("Tilemap", tilemap, typeof(Tilemap), true);
        floorSprite = (Sprite)EditorGUILayout.ObjectField("Floor Sprite", floorSprite, typeof(Sprite), false);
        wallSprite = (Sprite)EditorGUILayout.ObjectField("Wall Sprite", wallSprite, typeof(Sprite), false);
        startRoomSprite = (Sprite)EditorGUILayout.ObjectField("Start Sprite", startRoomSprite, typeof(Sprite), false);
        endRoomSprite = (Sprite)EditorGUILayout.ObjectField("End Sprite", endRoomSprite, typeof(Sprite), false);

        EditorGUILayout.Space(8);

        GUILayout.Label("Generation Settings", EditorStyles.miniBoldLabel);
        layoutSize = (LayoutSize)EditorGUILayout.EnumPopup("Layout Size", layoutSize);
        corridorOffset = EditorGUILayout.IntSlider("Corridor Offset", corridorOffset, 0, 10);

        EditorGUILayout.Space(4);

        GUILayout.Label("Seed", EditorStyles.miniBoldLabel);
        useRandomSeed = EditorGUILayout.Toggle("Random Seed", useRandomSeed);
        using (new EditorGUI.DisabledScope(useRandomSeed))
            seed = EditorGUILayout.IntField("Seed Value", seed);

        EditorGUILayout.Space(4);

        GUILayout.Label("Tile Tints", EditorStyles.miniBoldLabel);
        roomFloorColor = EditorGUILayout.ColorField("Room Floor", roomFloorColor);
        corridorColor = EditorGUILayout.ColorField("Corridor", corridorColor);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate", GUILayout.Height(30))) 
        {
            GenerateLevel();
        }

        if (GUILayout.Button("Clear")) 
        {
            ClearMap();
        }

        if (hasGenerated)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }
    }

    void ApplyDungeonSettings()
    {
        switch (layoutSize)
        {
            case LayoutSize.Small:
                width = 25; height = 25; roomCount = 3;
                roomMinSize = 5; roomMaxSize = 7; corridorThickness = 1;
                break;

            case LayoutSize.Medium:
                width = 40; height = 40; roomCount = 5;
                roomMinSize = 6; roomMaxSize = 9; corridorThickness = 1;
                break;

            case LayoutSize.Large:
                width = 55; height = 55; roomCount = 7;
                roomMinSize = 7; roomMaxSize = 11; corridorThickness = 1;
                break;
        }
    }

    // Check references
    bool ValidateInputs()
    {
        if (tilemap == null)
        {
            EditorUtility.DisplayDialog("Missing Reference", "Please assign a Tilemap before generating.", "OK");
            return false;
        }
        if (floorSprite == null)
        {
            EditorUtility.DisplayDialog("Missing Sprite", "Please assign a Floor Sprite before generating.", "OK");
            return false;
        }
        if (wallSprite == null)
        {
            EditorUtility.DisplayDialog("Missing Sprite", "Please assign a Wall Sprite before generating.", "OK");
            return false;
        }
        if (startRoomSprite == null)
        {
            EditorUtility.DisplayDialog("Missing Sprite", "Please assign a Start Room Sprite before generating.", "OK");
            return false;
        }
        if (endRoomSprite == null)
        {
            EditorUtility.DisplayDialog("Missing Sprite", "Please assign an End Room Sprite before generating.", "OK");
            return false;
        }

        return true;
    }

    // Generate level
    void GenerateLevel()
    {
        if (!ValidateInputs()) return;

        Undo.RegisterCompleteObjectUndo(tilemap, "Generate Dungeon");

        int activeSeed = useRandomSeed ? Random.Range(int.MinValue, int.MaxValue) : seed;
        Random.InitState(activeSeed);

        ApplyDungeonSettings();

        map = new int[width, height];
        rooms.Clear();

        GenerateRooms();
        ConnectRoomsWithMST();
        AssignStartAndEndRooms();
        GenerateWalls();
        CleanupIsolatedWalls();
        RenderMap();

        hasGenerated = true;
        statusMessage = $"Generated {rooms.Count} rooms  |  Seed: {activeSeed}  |  Size: {width}x{height}";

        SceneView.RepaintAll();
        Repaint();
    }
 
    // Room generation
    void GenerateRooms()
    {
        int attempts = 0;
        int maxAttempts = roomCount * 50;

        while (rooms.Count < roomCount && attempts < maxAttempts)
        {
            attempts++;

            int w = Random.Range(roomMinSize, roomMaxSize);
            int h = Random.Range(roomMinSize, roomMaxSize);
            int x = Random.Range(2, width - w - 2);
            int y = Random.Range(2, height - h - 2);

            RectInt newRoom = new RectInt(x, y, w, h);
            if (IsOverlapping(newRoom)) continue;

            rooms.Add(newRoom);
            DrawRoom(newRoom);
        }
    }

    bool IsOverlapping(RectInt newRoom)
    {
        foreach (var room in rooms)
        {
            RectInt expanded = new RectInt(room.x - roomPadding, room.y - roomPadding, room.width + roomPadding * 2, room.height + roomPadding * 2);

            if (newRoom.Overlaps(expanded)) return true;
        }
        return false;
    }

    void DrawRoom(RectInt room)
    {
        for (int x = room.x; x < room.xMax; x++)
            for (int y = room.y; y < room.yMax; y++)
                map[x, y] = 1;
    }

    // MST corridors
    void ConnectRoomsWithMST()
    {
        if (rooms.Count < 2) return;

        HashSet<int> inMST = new HashSet<int> { 0 };
        var edges = new List<(int from, int to, float dist)>();
        BuildEdgeList(edges);

        while (inMST.Count < rooms.Count)
        {
            float bestDist = float.MaxValue;
            int bestFrom = -1, bestTo = -1;

            foreach (var (from, to, dist) in edges)
            {
                bool fromIn = inMST.Contains(from);
                bool toIn = inMST.Contains(to);

                if (fromIn == toIn) continue;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestFrom = fromIn ? from : to;
                    bestTo = fromIn ? to : from;
                }
            }

            if (bestTo == -1) break; 

            inMST.Add(bestTo);
            CreateJitteredCorridor(GetCenter(rooms[bestFrom]), GetCenter(rooms[bestTo]));
        }
    }

    void BuildEdgeList(List<(int, int, float)> edges)
    {
        for (int i = 0; i < rooms.Count; i++)
            for (int j = i + 1; j < rooms.Count; j++)
                edges.Add((i, j, Vector2Int.Distance(GetCenter(rooms[i]), GetCenter(rooms[j]))));
    }

    Vector2Int GetCenter(RectInt room) => new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);

    void CreateJitteredCorridor(Vector2Int a, Vector2Int b)
    {
        int jx = (a.x + b.x) / 2 + Random.Range(-corridorOffset, corridorOffset + 1);
        int jy = (a.y + b.y) / 2 + Random.Range(-corridorOffset, corridorOffset + 1);
        jx = Mathf.Clamp(jx, 1, width - 2);
        jy = Mathf.Clamp(jy, 1, height - 2);

        Vector2Int mid = new Vector2Int(jx, jy);

        CarveLine(a, mid);
        CarveLine(mid, b);

        CarveEntryDoorway(a);
        CarveEntryDoorway(b);
    }

    void CarveLine(Vector2Int from, Vector2Int to)
    {
        int x = from.x, y = from.y;

        while (x != to.x) { CarveCorridor(x, y); x += (to.x > x) ? 1 : -1; }
        while (y != to.y) { CarveCorridor(x, y); y += (to.y > y) ? 1 : -1; }
        CarveCorridor(x, y);
    }

    void CarveCorridor(int x, int y)
    {
        for (int dx = -corridorThickness; dx <= corridorThickness; dx++)
        {
            for (int dy = -corridorThickness; dy <= corridorThickness; dy++)
            {
                int nx = x + dx, ny = y + dy;
                if (!IsInBounds(nx, ny)) continue;
                if (map[nx, ny] == 0)
                    map[nx, ny] = 5;
            }
        }
    }

    void CarveEntryDoorway(Vector2Int point)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = point.x + dx, ny = point.y + dy;
                if (!IsInBounds(nx, ny)) continue;
                if (map[nx, ny] == 0)
                    map[nx, ny] = 5;
            }
        }
    }

    // Start and end rooms
    void AssignStartAndEndRooms()
    {
        if (rooms.Count == 0) return;

        startRoom = GetLargestRoom();
        Vector2Int startCenter = GetCenter(startRoom);

        float maxDist = -1f;
        RectInt farthest = startRoom;

        foreach (var room in rooms)
        {
            float d = Vector2Int.Distance(startCenter, GetCenter(room));
            if (d > maxDist) { maxDist = d; farthest = room; }
        }

        endRoom = farthest;
        MarkRoom(startRoom, 2);
        MarkRoom(endRoom, 3);
    }

    RectInt GetLargestRoom()
    {
        RectInt largest = rooms[0];
        int largestArea = largest.width * largest.height;

        foreach (var room in rooms)
        {
            int area = room.width * room.height;
            if (area > largestArea) { largest = room; largestArea = area; }
        }

        return largest;
    }

    void MarkRoom(RectInt room, int type)
    {
        for (int x = room.x; x < room.xMax; x++)
            for (int y = room.y; y < room.yMax; y++)
                map[x, y] = type;
    }

    // Walls
    void GenerateWalls()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (map[x, y] == 0 && HasAdjacentFloor(x, y))
                    map[x, y] = 4;
    }

    void CleanupIsolatedWalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] != 4) continue;
                if (!HasAdjacentWall(x, y))
                    map[x, y] = 0;
            }
        }
    }
    bool HasAdjacentFloor(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                if (IsFloor(x + dx, y + dy)) return true;
            }
        return false;
    }
    bool HasAdjacentWall(int x, int y)
    {
        return IsWall(x + 1, y) || IsWall(x - 1, y) ||
               IsWall(x, y + 1) || IsWall(x, y - 1);
    }

    bool IsFloor(int x, int y)
    {
        if (!IsInBounds(x, y)) return false;
        int v = map[x, y];
        return v == 1 || v == 2 || v == 3 || v == 5;
    }

    bool IsWall(int x, int y)
    {
        if (!IsInBounds(x, y)) return false;
        return map[x, y] == 4;
    }

    // Render
    void RenderMap()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                switch (map[x, y])
                {
                    case 1: SetTile(x, y, floorSprite, roomFloorColor); break;
                    case 2: SetTile(x, y, startRoomSprite, roomFloorColor); break;
                    case 3: SetTile(x, y, endRoomSprite, roomFloorColor); break;
                    case 4: SetTile(x, y, wallSprite, Color.white); break;
                    case 5: SetTile(x, y, floorSprite, corridorColor); break;
                }
            }
        }
    }

    void SetTile(int x, int y, Sprite sprite, Color color)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.color = color;
        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
    }
   
    bool IsInBounds(int x, int y) =>
        x >= 0 && y >= 0 && x < width && y < height;

    void ClearMap()
    {
        if (tilemap == null)
        {
            EditorUtility.DisplayDialog("Nothing to Clear", "No Tilemap is assigned.", "OK");
            return;
        }

        Undo.RegisterCompleteObjectUndo(tilemap, "Clear Level");
        tilemap.ClearAllTiles();

        hasGenerated = false;
        statusMessage = "";
        Repaint();
    }
}