using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        running,
        gameover,
        mainMenu
    }

    public static GameController Instance
    {
        get;
        protected set;
    }

    public GameState gameState = GameState.mainMenu;

    public bool isPaused = false;

    public MenuManager menuManager;

    // ** PREFABS
    public GameObject nonPlayableTile;
    public GameObject naturalCanalTile;
    public GameObject townTile;
    public GameObject lavaCanalTile;
    public GameObject lavaCanalHorizontalTile;
    public GameObject canalDownLeft;
    public GameObject canalDownRight;
    public GameObject canalUpLeft;
    public GameObject canalUpRight;


    public Dictionary<Tile.Type, GameObject> tilePrefabMap;

    // *********************

    // ** tile sprites ****
    public Sprite townSprite;
    public Sprite naturalSprite;
    public Sprite canalSprite;
    public Sprite canalSpriteHorizontal;
    public Sprite canalDownLeftSprite;
    public Sprite canalDownRightSprite;
    public Sprite canalUpLeftSprite;
    public Sprite canalUpRightSprite;
    public Dictionary<Tile.Type, Sprite> tileSprites;
    // *****

    public GameObject gameTileHolder;

    public Texture2D backgroundTexture;
    private Texture2D backgroundTextureClone;
    public GameObject background;

    public Texture2D buidableTileTexture;
    private Texture2D buildableTileTextureClone;

    public Texture2D townTileTexture;
    private Texture2D townTileTextureClone;

    public Texture2D noGridTexture;
    private Texture2D noGridTextureClone;

    public Texture2D lavaPathTexture;
    private Texture2D lavaPathTextureClone;

    [SerializeField]
    private int backgroundWidth;
    [SerializeField]
    private int backgroundHeight;

    [SerializeField]
    private float dirtMinLum = 0.4f;

    [SerializeField]
    private float dirtMaxLum = 0.9f;

    private static int fullWidth = 1024;
    private static int fullHeight = 768;
    private static int tileSize = 64;
    private static int leftMenuWidth = tileSize * 4;   // 5 tiles wide
    //private static int topBarHeight = tileSize * 3;
    private static int topBarHeight = 0;
    //private static int bottomBarHeight = tileSize * 2;
    private static int bottomBarHeight = 0;
    private int gameAreaLeft = leftMenuWidth;
    private int gameAreaTop = topBarHeight;
    private int gameAreaWidth = fullWidth - leftMenuWidth - tileSize;   // tilesize to make space for a tile width at the end
    private int gameAreaHeight = fullHeight - topBarHeight - bottomBarHeight;

    private int gameAreaTilesWide;
    private int gameAreaTilesHigh;

    private int tileSizeMultiplier = 2;

    public Tile[,] tiles;
    public List<Tile> townTiles = new List<Tile>();    // store all the town tiles for easy checking of game over state later
    public Dictionary<GameObject, Tile> tileMap = new Dictionary<GameObject, Tile>();

    public bool gameStarted = false;
    public bool lavaFlowing = false;

    public float lavaFlowSpeed = 1;
    public float levelLavaFlowSpeed = 1;

    public Tile currentFlowingTile = null;

    public GameObject placingTile;

    public event Action<int> OnScoreChange = delegate { };
    public int score = 0;      // 'survivors' - goes up incrementally every specified amount of time
    private int scoreIncrement = 1;
    private float secondsToScore = 0.25f;    // every 10 seconds of play time the survivor count will go up

    // ** shop

    public GameObject shopOne;
    public GameObject shopTwo;
    public GameObject shopThree;

    public ShopPreview shopPreview;

    List<Tile.Type> purchaseableTypes = new List<Tile.Type>() { Tile.Type.canal, Tile.Type.canal_horizontal, Tile.Type.canal_down_left, Tile.Type.canal_down_right,
                                                                Tile.Type.canal_up_left, Tile.Type.canal_up_right };
    List<Tile> tilesComingUp = new List<Tile>();
    List<Tile> tilesForPurchase = new List<Tile>();

    public event Action<int> OnUpdateShopTimer = delegate { };
    int shopTimer;
    int shopTimerDuration = 5;  // 5 seconds

    public event Action<int> OnUpdatePlayerFunds = delegate { };
    public int playerStartingFunds = 4000;
    public int playerFunds;

    public int skipPrice = 500;


    // *********

    List<int> highScores = new List<int>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be two GameController");
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hello Unity Console");

        tileSprites = new Dictionary<Tile.Type, Sprite>() { { Tile.Type.town, townSprite }, { Tile.Type.natural, naturalSprite }, { Tile.Type.canal, canalSprite },
            { Tile.Type.canal_horizontal, canalSpriteHorizontal },
            { Tile.Type.canal_down_left, canalDownLeftSprite },
            { Tile.Type.canal_down_right, canalDownRightSprite },
            { Tile.Type.canal_up_left, canalUpLeftSprite },
            { Tile.Type.canal_up_right, canalUpRightSprite }
        };

        tilePrefabMap = new Dictionary<Tile.Type, GameObject>() { { Tile.Type.canal, lavaCanalTile },
            { Tile.Type.canal_horizontal, lavaCanalHorizontalTile },
            { Tile.Type.canal_down_left, canalDownLeft },
            { Tile.Type.canal_down_right, canalDownRight },
            { Tile.Type.canal_up_left, canalUpLeft },
            { Tile.Type.canal_up_right, canalUpRight }
        };

        // create all of the tiles and fill the game area
        gameAreaTilesHigh = gameAreaHeight / (tileSize);
        gameAreaTilesWide = gameAreaWidth / (tileSize);
        Debug.Log("game are tiles high:" + gameAreaTilesHigh + " game area tiles wide:" + gameAreaTilesWide);

        //setupGame();
        setupBackground();
    }

    private void setupGame()
    {
        // reset some things
        StopAllCoroutines();

        score = 0;

        playerFunds = playerStartingFunds;
        OnUpdatePlayerFunds(playerFunds);

        townTiles.Clear();
        foreach(Tile t in tileMap.Values)
        {
            Destroy(t.go);
        }
        tileMap.Clear();
        Destroy(gameTileHolder);
        gameTileHolder = new GameObject("GameTileHolder");

        // setup first purchaseable tiles in shop
        List<Tile> shopTiles = generateNewTilesForPurchase(3);
        setupShop(shopTiles);

        // build playing area
        tiles = new Tile[gameAreaTilesWide, gameAreaTilesHigh];
        for (int x = 0; x < gameAreaTilesWide * tileSizeMultiplier; x += tileSizeMultiplier)
        {
            for (int y = 0; y < gameAreaTilesHigh * tileSizeMultiplier; y += tileSizeMultiplier)
            {
                GameObject emptyTile = null;

                Tile tile = null;

                // set if town tile
                bool isTown = (y / tileSizeMultiplier) < (2);
                bool isVolcano = (y / tileSizeMultiplier) > gameAreaTilesHigh - 3;
                if (isTown)
                {
                    tile = Tile.createTown(x / tileSizeMultiplier, y / tileSizeMultiplier);
                    townTiles.Add(tile);
                    // change tile to town tile
                    emptyTile = placePathTile(tile, townTile);
                }
                else
                if (isVolcano)
                {
                    if (x / 2 == Mathf.RoundToInt(gameAreaTilesWide / 2f) - 1)
                    {
                        tile = Tile.createNaturalCanalVertical(x / tileSizeMultiplier, y / tileSizeMultiplier);
                        // tile is volcano default path
                        if (y == ((gameAreaTilesHigh-1) * tileSizeMultiplier) - tileSizeMultiplier)
                        {
                            // top most volcano tile is where lava starts
                            tile.flowStartedFromDirection = "up";
                            tile.isFlowing = true;
                            tile.flowStartTime = Time.time;
                        }
                        emptyTile = placePathTile(tile, naturalCanalTile);
                        setTileNotBuildable(tile, emptyTile);
                    }
                    else
                    {
                        tile = Tile.createNaturalCanalVertical(x / tileSizeMultiplier, y / tileSizeMultiplier);
                        emptyTile = placePathTile(tile, naturalCanalTile);
                        setTileNotBuildable(tile, emptyTile);
                    }
                }
                else
                if (x / 2 == Mathf.RoundToInt(gameAreaTilesWide / 2f) - 1)
                {
                    // tile is volcano default path
                    tile = Tile.createNaturalCanalVertical(x / tileSizeMultiplier, y / tileSizeMultiplier);
                    emptyTile = placePathTile(tile, naturalCanalTile);
                }
                else
                {
                    // empty ground tiles
                    tile = Tile.createNaturalCanalVertical(x / tileSizeMultiplier, y / tileSizeMultiplier);
                    emptyTile = placePathTile(tile, naturalCanalTile);
                }

                emptyTile.transform.position = new Vector3(x + gameAreaLeft * tileSizeMultiplier / tileSize, y + bottomBarHeight * tileSizeMultiplier / tileSize, 0);
                tile.go = emptyTile;
                tile.go.transform.SetParent(gameTileHolder.transform);
                tiles[x / tileSizeMultiplier, y / tileSizeMultiplier] = tile;
                tileMap.Add(emptyTile, tile);
            }
        }

        foreach (Tile t in tiles)
        {
            setTileConnections(t);
        }
    }

    private void setupShop(List<Tile> shopTiles)
    {
        setShopTiles(shopTiles);

        List<Tile> previewTiles = generateNewTilesForPurchase(3);
        shopPreview.updatePreviews(previewTiles);

        shopTimer = shopTimerDuration;
        
        //OnUpdateShopTimer(shopTimer);
    }

    private void setupBackground()
    {
        createSprite(backgroundTexture, backgroundTextureClone, background, 32);

        SpriteRenderer sr = background.GetComponent<SpriteRenderer>();
        backgroundWidth = (int)sr.sprite.rect.width;
        backgroundHeight = (int)sr.sprite.rect.height;

        fillBackground();
    }

    private GameObject placePathTile(Tile tile, GameObject spriteGO)
    {
        return Instantiate(spriteGO);
    }

    private void setTileConnections(Tile tile)
    {
        // clear existing connections
        tile.connections.Clear();

        Tile tileUp = tile.y + 1 < gameAreaTilesHigh ? tiles[tile.x, tile.y + 1] : null;
        Tile tileDown = tile.y - 1 >= 0 ? tiles[tile.x, tile.y - 1] : null;
        Tile tileLeft = tile.x - 1 >= 0 ? tiles[tile.x - 1, tile.y] : null;
        Tile tileRight = tile.x + 1 < gameAreaTilesWide ? tiles[tile.x + 1, tile.y] : null;

        if (tile.canConnect["up"])
        {
            tile.connections["up"] = tileUp;
        }
        if (tile.canConnect["down"])
        {
            tile.connections["down"] = tileDown;
        }
        if (tile.canConnect["left"])
        {
            tile.connections["left"] = tileLeft;
        }
        if (tile.canConnect["right"])
        {
            tile.connections["right"] = tileRight;
        }
    }

    private void createSprite(Texture2D texture, Texture2D textureClone, GameObject spriteGO, int pixelsPerUnit)
    {
        textureClone = Instantiate(texture);
        Sprite sprite = Sprite.Create(textureClone, new Rect(0, 0, texture.width, texture.height), Vector2.zero, pixelsPerUnit);
        SpriteRenderer sr = spriteGO.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
    }

    public void placeTile(int x, int y, Tile.Type type)
    {
        // change tile to placed tile
        Tile currentTile = tiles[x, y];
        if(!currentTile.isBuildable || currentTile.isFlowing)
        {
            Debug.Log("not buildable");
            return;
        }
        setTileNotBuildable(currentTile, currentTile.go);

        currentTile.setTileByType(type);

        Debug.Log("placing type: " + type);
        Debug.Log(tileSprites[type].name);
        placingTile.transform.position = currentTile.go.transform.position;
        tileMap.Remove(currentTile.go);
        Destroy(currentTile.go);
        currentTile.go = Instantiate(placingTile);

        tileMap.Add(currentTile.go, currentTile);

        setTileConnections(currentTile);

        // remove placingTile so you can't place another one
        Destroy(placingTile);
        placingTile = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            startGameRoutines();
            gameStarted = false;
        }

        if (gameState == GameState.running)
        {
            if (!isPaused)
            {
                if (placingTile != null)
                {
                    // have the tile stick to the mouse cursor
                    Vector3 camScreenVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    placingTile.transform.position = new Vector3(camScreenVector.x - (tileSizeMultiplier / 2), camScreenVector.y - (tileSizeMultiplier / 2), -0.1f);


                    if (Input.GetMouseButtonDown(1))
                    {
                        Destroy(placingTile);
                        placingTile = null;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                toggleInGameMenu();
            }
        }

        if(Input.GetKey(KeyCode.BackQuote))
        {
            lavaFlowSpeed = 12;
        }
        if (Input.GetKeyUp(KeyCode.BackQuote))
        {
            lavaFlowSpeed = levelLavaFlowSpeed;
        }
    }

    private void startGameRoutines()
    {
        StartCoroutine(runGame());
        StartCoroutine(incrementScore());
        StartCoroutine(decrementShopTimer());
    }

    IEnumerator decrementShopTimer()
    {
        while (lavaFlowing && !isPaused)
        {
            OnUpdateShopTimer(shopTimer);
            shopTimer--;
            if(shopTimer < 0)
            {
                // update shop with preview items;
                setupShop(shopPreview.previewTiles);
            }
            yield return new WaitForSecondsRealtime(1);
        }
        yield return null;
    }
    IEnumerator incrementScore()
    {
        OnScoreChange(score);
        yield return new WaitForSecondsRealtime(0.5f);
        // TODO: speed up based on the game speed
        while(lavaFlowing && !isPaused)
        {
            OnScoreChange(score);
            score += scoreIncrement;
            yield return new WaitForSecondsRealtime(secondsToScore);
        }
        yield return null;
    }

    IEnumerator runGame()
    {
        while (lavaFlowing && !isPaused && gameState == GameState.running)
        {
            foreach (Tile tile in tiles)
            {
                //if (tile.isFlowing && !tile.isFull)
                if(tile.isFlowing)
                {
                    // if tile is flowing again but is already full, let it 'overflow' on all directions!
                    // this can cause a cascade effect
                    if (tile.isFull)
                    {
                        Debug.Log("tile " + tile.go.name + " is full already, cascading out to neighbors!");
                        Tile tileUp = tile.y + 1 < gameAreaTilesHigh ? tiles[tile.x, tile.y + 1] : null;
                        Tile tileDown = tile.y - 1 >= 0 ? tiles[tile.x, tile.y - 1] : null;
                        Tile tileLeft = tile.x - 1 >= 0 ? tiles[tile.x - 1, tile.y] : null;
                        Tile tileRight = tile.x + 1 < gameAreaTilesWide ? tiles[tile.x + 1, tile.y] : null;

                        if (tileUp != null)
                        {
                            tileUp.setFlowing("down");
                        }
                        if (tileDown != null)
                        {
                            tileDown.setFlowing("up");
                        }
                        if (tileLeft != null)
                        {
                            tileLeft.setFlowing("right");
                        }
                        if (tileRight != null)
                        {
                            tileRight.setFlowing("left");
                        }
                        continue;
                    }

                    if (tile.go.GetComponent<TileScript>() != null)
                    {
                        // fill the tile
                        tile.fillValue += (Time.deltaTime * lavaFlowSpeed);

                        // figure out which lava direction to animate based on which direction the lava entered the tile..
                        // animate the lava icon to appear "filling" the tile
                        /*
                        SpriteRenderer lavaSr = tile.go.GetComponent<TileScript>().getLavaSpriteByDirection(tile.flowStartedFromDirection);
                        lavaSr.size = new Vector2(lavaSr.size.x, -1 * (tileSizeMultiplier * tile.fillValue));   // -1 * because the stretching of the icon looks better in reverse
                        */
                        tile.go.GetComponent<TileScript>().animateLava(tile, tileSizeMultiplier);

                        // stop the flow because tile has filled up
                        if (tile.fillValue >= 1)
                        {
                            tile.isFull = true;
                            tile.isFlowing = false;     // for the apocalypse comment this out

                            if (tile.type != Tile.Type.natural && tile.type != Tile.Type.nonPlayable && tile.type != Tile.Type.town)
                            {
                                addFunds(500);
                            }

                            // check for game over state here
                            if(checkGameOver())
                            {
                                triggerGameOver();
                                yield return null;
                            }

                            // start the connected tiles
                            foreach (string direction in tile.connections.Keys)
                            {
                                // you need to be able to spill into any tile as long as you are connected to it, regardless of it being connected to you.
                                //  if you can't do this then the player can just 'block' lava flow and delay forever
                                // but we only want to output in the direction of the flow
                                //if (tile.connections[direction] != null && direction == tile.getOutputDirection())
                                if (tile.canConnect[direction] && direction != tile.flowStartedFromDirection)
                                {
                                    if(tile.connections[direction] == null || tile.connections[direction].type == Tile.Type.nonPlayable)
                                    {
                                        // if the connection tile is null then we are flowing the lava into out of bounds, overflow time!
                                        tile.setFlowing(direction);
                                        continue;
                                    }
                                    Tile newFlow = tile.connections[direction];
                                    newFlow.setFlowing(direction);
                                    setTileNotBuildable(newFlow, newFlow.go);

                                    // if lava is output to an empty natural space if will automatically flow down even if it is output from below.
                                    if (newFlow.type == Tile.Type.natural)
                                    {
                                        newFlow.flowStartedFromDirection = "up";
                                    }
                                } 
                            }
                        }
                    }
                }
            }

            //yield return new WaitForSecondsRealtime(1);
            yield return null;
        }
    }

    private void setTileNotBuildable(Tile tile, GameObject go)
    {
        tile.isBuildable = false;
        if (go.GetComponent<TileScript>().gridMarker != null)
        {
            go.GetComponent<TileScript>().gridMarker.enabled = false;
        }
    }

    private bool checkGameOver()
    {
        int destroyedCount = 0;
        // check all 'town' tiles to see if they are all 'full' of lava
        foreach(Tile t in townTiles)
        {
            if (t.isFull)
            {
                destroyedCount++;
            }
        }
        if(destroyedCount >= townTiles.Count)
        {
            return true;
        }
        return false;
    }

    private void triggerGameOver()
    {
        pauseGame();
        gameState = GameState.gameover;
        menuManager.openPage(MenuPage.Page.gameover);
        submitScore();
    }

    private void submitScore()
    {
        List<int> newHighScores = new List<int>();
        int scoreIndex = highScores.Count;  // out of bounds to start
        for(int i = 0; i < 10; i++)
        {
            if(i == highScores.Count)
            {
                newHighScores.Add(score);
                break;
            }
            if(score > highScores[i])
            {
                scoreIndex = i;
                newHighScores.Add(score);
                break;
            } else
            {
                newHighScores.Add(highScores[i]);
            }
        }

        for(int i = scoreIndex+1; i < highScores.Count; i++)
        {
            newHighScores.Add(highScores[i-1]);
        }
        highScores = newHighScores;
    }

    public bool gameRunningAndNotPaused()
    {
        return gameState == GameState.running && !isPaused;
    }

    private void OnValidate()
    {
        //fillBackground();
    }

    public void startGame()
    {
        setupGame();

        gameStarted = true;
        lavaFlowing = true;
        isPaused = false;

        menuManager.openPage(MenuPage.Page.game);

        gameState = GameState.running;
    }

    public void returnToMenu()
    {
        gameStarted = false;
        lavaFlowing = false;

        gameState = GameState.mainMenu;
        menuManager.openPage(MenuPage.Page.main);
    }

    public void viewHighScores()
    {
        menuManager.openPage(MenuPage.Page.highscores);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void pauseGame()
    {
        isPaused = true;
        lavaFlowing = false;
        Debug.Log("Game paused");
    }

    public void unpauseGame()
    {
        lavaFlowing = true;
        Debug.Log("Game unpaused");
        isPaused = false;
        startGameRoutines();
    }

    public void toggleInGameMenu()
    {
        if (menuManager.currentPage == MenuPage.Page.gameMenu)
        {
            unpauseGame();
            menuManager.openPage(MenuPage.Page.game);
        }
        else
        {
            pauseGame();
            menuManager.openPage(MenuPage.Page.gameMenu);
        }
    }

    public List<Tile> generateNewTilesForPurchase(int amount)
    {
        List<Tile> tiles = new List<Tile>();
        List<int> usedIndexes = new List<int>();
        for (int i = 0; i < amount; i++)
        {
            int index = UnityEngine.Random.Range(0, purchaseableTypes.Count - 1);
            if (usedIndexes.Contains(index))
            {   
                // skip duplicates and re-roll
                i--;
                continue;
            }
            usedIndexes.Add(index);
            Tile.Type tileType = purchaseableTypes[index];
            Tile tile = new Tile();
            tile.setTileByType(tileType);
            tiles.Add(tile);
        }

        return tiles;
    }

    private void setShopTiles(List<Tile> shopTiles)
    {
        // this should be made more general so you can chance the number of tiles for purchase easily.  But whatever hard-coding this because I am running out of time
        shopOne.GetComponent<TilePurchase>().tileDisplay.GetComponent<SpriteRenderer>().sprite = tileSprites[shopTiles[0].type];
        shopOne.GetComponent<TilePurchase>().tilePurchase = tilePrefabMap[shopTiles[0].type];

        shopTwo.GetComponent<TilePurchase>().tileDisplay.GetComponent<SpriteRenderer>().sprite = tileSprites[shopTiles[1].type];
        shopTwo.GetComponent<TilePurchase>().tilePurchase = tilePrefabMap[shopTiles[1].type];

        shopThree.GetComponent<TilePurchase>().tileDisplay.GetComponent<SpriteRenderer>().sprite = tileSprites[shopTiles[2].type];
        shopThree.GetComponent<TilePurchase>().tilePurchase = tilePrefabMap[shopTiles[2].type];
    }

    public void skipShopTiles()
    {
        if (playerFunds - skipPrice >= 0)
        {
            addFunds(-1 * skipPrice);
            setupShop(shopPreview.previewTiles);
        }
    }

    private void addFunds(int amount)
    {
        playerFunds += amount;
        OnUpdatePlayerFunds(playerFunds);
    }

    public void buyTile(GameObject tilePurchase)
    {
        if (gameRunningAndNotPaused())
        {
            if(playerFunds - 400 < 0)
            {
                // can't afford
                return;
            }
            GameObject tile = Instantiate(tilePurchase);
            tile.GetComponent<Collider2D>().enabled = false;
            placingTile = tile;

            playerFunds -= 400;
            OnUpdatePlayerFunds(playerFunds);
        }
    }

    public List<int> getHighScores()
    {
        return highScores;
    }

    private void fillBackground()
    {
        Texture2D tex = background.GetComponent<SpriteRenderer>().sprite.texture;
        for (int x = 0; x <= backgroundWidth; x++)
        {
            for (int y = 0; y <= backgroundHeight; y++)
            {
                float h, s, v;
                Color.RGBToHSV(tex.GetPixel(x, y), out h, out s, out v);
                //Debug.Log("h=" + h + " s=" + s + " v=" + v);
                //v = Random.Range(dirtMinHum, dirtMaxLum);
                //v = Mathf.Clamp(Mathf.PerlinNoise(x, y), dirtMinLum, dirtMaxLum);
                // we need to divide the current xcoord/ycoord by the width/height because Mathf.PerlinNoise requires a float between 0..1
                v = Mathf.PerlinNoise(x / (float)backgroundWidth * 10, y / (float)backgroundHeight * 10);   // make the ground look "natural"
                v += UnityEngine.Random.Range(-0.2f, 0.2f); // give some "dirty" texture to it
                v = Mathf.Clamp(v, dirtMinLum, dirtMaxLum);
                //Debug.Log(x / 50f);
                //Debug.Log(Mathf.PerlinNoise(x/50f, y/50f));

                Color newColor = Color.HSVToRGB(h, s, v);
                tex.SetPixel(x, y, newColor);
            }
        }
        tex.Apply();
    }
}
