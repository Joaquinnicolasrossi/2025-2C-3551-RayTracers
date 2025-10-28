using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace TGC.MonoGame.TP;

/// <summary>
///     Esta es la clase principal del juego.
///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
/// </summary>
public class TGCGame : Game
{

    #region Variables del juego

    #region Paths
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderMusic = "Music/";
    public const string ContentFolderSounds = "Sounds/";
    public const string ContentFolderSpriteFonts = "SpriteFonts/";
    public const string ContentFolderTextures = "Textures/";
    #endregion

    #region Logica del Auto
    private Vector3 _carPosition;
    private float _carRotation = 0f;
    private BoundingSphere _carBoundingSphere;
    private const float CarBoundingSphereRadius = 20f;

    // Car's movement variables (need to be adjusted)
    private float _carSpeed = 0f;
    private float MaxSpeed = 300f;
    private float Acceleration = 100f;
    private float BrakeDeceleration = 200f;
    private const float TurnSpeed = 60f;
    private float DriftFactor = 0.95f; // 0 (no drift) > DriftFactor > 1 (no adhesion)
    private Vector3 _carDirection = Vector3.Forward;
    #endregion

    #region HUD
    private int _score = 0;
    private float _fuel = 100f;
    private int _wrenches = 0;
    private float _health = 100f;
    private bool _gameOver = false;
    private SpriteFont _gameOverFont;
    private SpriteFont _mainFont;
    private Texture2D _coinIcon;
    private Texture2D _wrenchIcon;
    private Texture2D _gasIcon;
    private Texture2D _pixelTexture;
    private SpriteBatch _spriteBatch;
    #endregion

    #region Circuito
    // Pista Modelo
    private QuadPrimitive _floor;
    private Matrix _floorWorld;

    // Pista generada a mano
    private QuadPrimitive _road;
    private Matrix _roadWorld;
    private float _roadLength;
    private float _roadWidth;
    private QuadPrimitive _line;
    private Matrix _lineWorld;
    private float _lineSpacing;
    private float _lineLength;
    private float _lineWidth;
    #endregion

    #region Modelos
    private Color _selectedCarColor;
    private Model _selectedCarModel;
    private Model _racingCarModel;
    private Model _f1CarModel;
    private Model _cybertruckModel;
    private Model _houseModel;
    private Model _plantModel;
    private Model _rockModel;
    private Model _treeModel;
    private Model _trackModel;
    private Model _gasModel;
    private Model _wrenchModel;
    private Model _coinModel;
    private Model _deerModel;
    private Model _goatModel;
    private Model _cowModel;

    public enum TerrainType
    {
        Asphalt = 0,
        Dirt = 1,
        Snow = 2
    }

    private TerrainType _currentTerrain;
    private Random _random = new Random();
    #endregion

    #region Matrices de mundo
    private Matrix _carWorld;
    List<Matrix> casasWorld = new List<Matrix>();
    List<Matrix> plantasWorld = new List<Matrix>();
    List<Matrix> piedrasWorld = new List<Matrix>();
    private Matrix trackWorld;
    #endregion

    #region Texturas
    private Texture _grassTexture;
    private Texture2D _roadTexture;
    #endregion

    #region Shaders
    private Effect _basicShader;
    private Effect _grassShader;
    #endregion

    #region Menu
    public const int ST_PRESENTACION = 0;
    public const int ST_SELECCION = 1;
    public const int ST_STAGE_1 = 2;
    public int status = ST_PRESENTACION;
    private Texture2D _menuSelection;
    private Texture2D _menuStart;
    #endregion

    #region Camara
    private readonly GraphicsDeviceManager _graphics;
    private Camera _camera;
    #endregion

    #region Spawns
    private List<Vector3> _collectibleSpawnPoints = new List<Vector3>();
    private List<Collectible> _collectibles = new List<Collectible>();
    private List<Vector3> _obstacleSpawnPoints = new List<Vector3>();
    private List<Obstacle> _obstacles = new List<Obstacle>();
    #endregion

    #region Modo debug
    private Model _debugSphereModel;
    private RasterizerState _solidRasterizerState;
    private RasterizerState _wireframeRasterizerState;
    private bool _isDebugModeEnabled = false;
    private KeyboardState _previousKeyboardState;
    #endregion

    #region Audio
    private Song _menuSong;
    private Song _gameplaySong;
    private SoundEffect _collectCoinSound;
    private SoundEffect _collectGasSound;
    private SoundEffect _collectWrenchSound;
    private SoundEffect _repairSound;
    private SoundEffect _crashLateralSound;
    private SoundEffect _crashFrontalSound;
    private SoundEffect _gameOverSound;
    #endregion

    #endregion

    /// <summary>
    ///     Constructor del juego.
    /// </summary>
    public TGCGame()
    {
        // Maneja la configuracion y la administracion del dispositivo grafico.
        _graphics = new GraphicsDeviceManager(this);

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;

        // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
        // Carpeta raiz donde va a estar toda la Media.
        Content.RootDirectory = "Content";
        // Hace que el mouse sea visible.
        IsMouseVisible = true;
    }

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
    ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void Initialize()
    {
        // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

        // Inicializo la camara
        _camera = new Camera(GraphicsDevice.Viewport.AspectRatio, 150f, 700f, -30f);

        // Configuramos nuestras matrices de la escena.
        _carWorld = Matrix.Identity;

        _floorWorld = Matrix.CreateScale(31000f) * Matrix.CreateTranslation(0f, 0f, 0f);

        // Configuro la ruta
        _roadLength = 3000f;
        _roadWidth = 70f;
        _lineSpacing = 100f;
        _lineLength = 30f;
        _lineWidth = 5f;

        _roadWorld = Matrix.CreateScale(_roadWidth, 1f, _roadLength) * Matrix.CreateTranslation(new Vector3(0f, 0.5f, 0.00f)); // 0.02 para evitar z-fighting

        // Inicializo el auto en el principio de la ruta de modelo
        _carPosition = new Vector3(0f, 0f, _roadLength - 200f);

        trackWorld = Matrix.CreateScale(0.66f) * Matrix.CreateRotationY(-MathHelper.PiOver2)
                                                      * Matrix.CreateTranslation(-455, 1f, 3200);

        _currentTerrain = (TerrainType)_random.Next(0, 3);

        _carBoundingSphere.Center = _carPosition;
        _carBoundingSphere.Radius = CarBoundingSphereRadius;

        _solidRasterizerState = new RasterizerState();
        _wireframeRasterizerState = new RasterizerState
        {
            FillMode = FillMode.WireFrame, // Modo alambre
            CullMode = CullMode.None       // Dibuja ambas caras
        };

        _previousKeyboardState = Keyboard.GetState();

        base.Initialize();
    }

    /// <summary>
    ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
    ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
    ///     que podemos pre calcular para nuestro juego.
    /// </summary>
    protected override void LoadContent()
    {
        // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        #region HUD
        _mainFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "MainFont");
        _gameOverFont = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "GameOver");
        _coinIcon = Content.Load<Texture2D>(ContentFolderTextures + "HUD/coin_icon");
        _wrenchIcon = Content.Load<Texture2D>(ContentFolderTextures + "HUD/wrench_icon");
        _gasIcon = Content.Load<Texture2D>(ContentFolderTextures + "HUD/gas_icon");
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        #endregion

        #region Modelos
        _menuSelection = Content.Load<Texture2D>("Menus/menu_vehicle_selection");
        _menuStart = Content.Load<Texture2D>("Menus/menu_start");
        _racingCarModel = Content.Load<Model>(ContentFolder3D + "Cars/RacingCarA/RacingCar");
        _cybertruckModel = Content.Load<Model>(ContentFolder3D + "Cars/Cybertruck/Cybertruck1");
        _f1CarModel = Content.Load<Model>(ContentFolder3D + "Cars/F1/F1");
        _treeModel = Content.Load<Model>(ContentFolder3D + "Tree/Tree");
        _houseModel = Content.Load<Model>(ContentFolder3D + "Houses/Cabin");
        _trackModel = Content.Load<Model>(ContentFolder3D + "Track/road");
        _plantModel = Content.Load<Model>(ContentFolder3D + "Plants/Plant1/Low Grass");
        _rockModel = Content.Load<Model>(ContentFolder3D + "Rocks/Rock2/rock");
        _gasModel = Content.Load<Model>(ContentFolder3D + "Collectables/Gas/Gas");
        _wrenchModel = Content.Load<Model>(ContentFolder3D + "Collectables/Wrench/Wrench");
        _coinModel = Content.Load<Model>(ContentFolder3D + "Collectables/Coin/Coin");
        _cowModel = Content.Load<Model>(ContentFolder3D + "Animals/cow");
        _deerModel = Content.Load<Model>(ContentFolder3D + "Animals/deer");
        _goatModel = Content.Load<Model>(ContentFolder3D + "Animals/goat");
        #endregion

        #region Shaders
        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        _basicShader = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
        _grassShader = Content.Load<Effect>(ContentFolderEffects + "GrassShader");

        // Asigno el efecto que cargue a cada parte del mesh.
        ModelDrawingHelper.AttachEffectToModel(_racingCarModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_cybertruckModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_f1CarModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_treeModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_houseModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_trackModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_plantModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_rockModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_gasModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_wrenchModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_coinModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_cowModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_deerModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_goatModel, _basicShader);
        #endregion

        #region Terrenos
        switch (_currentTerrain)
        {
            case TerrainType.Asphalt:
                _grassTexture = Content.Load<Texture2D>(ContentFolderTextures + "Asphalt/OffRoad/grassTextureV2");
                _roadTexture = Content.Load<Texture2D>(ContentFolderTextures + "Asphalt/Road/asphaltColor");
                // MaxSpeed = 300f;
                // Acceleration = 100f;
                // BrakeDeceleration = 250f;
                // DriftFactor = 0.75f;
                break;
            case TerrainType.Dirt:
                _grassTexture = Content.Load<Texture2D>(ContentFolderTextures + "Dirt/OffRoad/grassTextureV2");
                _roadTexture = Content.Load<Texture2D>(ContentFolderTextures + "Dirt/Road/dirtTexture");
                // MaxSpeed = 200f;
                // Acceleration = 80f;
                // BrakeDeceleration = 150f;
                // DriftFactor = 0.96f;
                break;
            case TerrainType.Snow:
                _grassTexture = Content.Load<Texture2D>(ContentFolderTextures + "Snow/OffRoad/snowColor");
                _roadTexture = Content.Load<Texture2D>(ContentFolderTextures + "Snow/Road/snowDirtColor");
                // MaxSpeed = 180f;
                // Acceleration = 70f;
                // BrakeDeceleration = 100f;
                // DriftFactor = 0.97f;
                break;
        }
        #endregion

        #region Mapa (quads)
        _floor = new QuadPrimitive(GraphicsDevice);
        _road = new QuadPrimitive(GraphicsDevice);
        _line = new QuadPrimitive(GraphicsDevice);
        #endregion

        #region Spawns
        // Spawn de objetos segun el nombre de los empties de road.fbx
        foreach (ModelBone bone in _trackModel.Bones)
        {
            if (bone.Name.StartsWith("casa"))
            {
                Vector3 pos = bone.Transform.Translation;
                Matrix casaBase = Matrix.CreateScale(0.6f);
                Matrix world = casaBase * Matrix.CreateTranslation(pos) * trackWorld;
                casasWorld.Add(world);
            }
            else if (bone.Name.StartsWith("piedra"))
            {
                Vector3 pos = bone.Transform.Translation;
                Matrix piedraBase = Matrix.CreateScale(40f);
                Matrix world = piedraBase * Matrix.CreateTranslation(pos) * trackWorld;
                piedrasWorld.Add(world);
            }
            else if (bone.Name.StartsWith("planta"))
            {
                Vector3 pos = bone.Transform.Translation;
                Matrix plantaBase = Matrix.CreateScale(5f);
                Matrix world = plantaBase * Matrix.CreateTranslation(pos) * trackWorld;
                plantasWorld.Add(world);
            }
            else if (bone.Name.StartsWith("collectable"))
            {
                Vector3 spawnPosition = bone.Transform.Translation;
                Vector3 worldSpawnPosition = Vector3.Transform(spawnPosition, trackWorld);
                _collectibleSpawnPoints.Add(worldSpawnPosition);
            }
            else if (bone.Name.StartsWith("obstacle"))
            {
                Vector3 spawnPosition = bone.Transform.Translation;
                Vector3 worldSpawnPosition = Vector3.Transform(spawnPosition, trackWorld);
                _obstacleSpawnPoints.Add(worldSpawnPosition);
            }
        }

        SpawnCollectiblesAndObstacles();
        #endregion

        #region Debug
        _debugSphereModel = Content.Load<Model>(ContentFolder3D + "Debug/SphereDebug");
        ModelDrawingHelper.AttachEffectToModel(_debugSphereModel, _basicShader);
        #endregion

        #region Audio
        _menuSong = Content.Load<Song>(ContentFolderMusic + "menu_music");
        _gameplaySong = Content.Load<Song>(ContentFolderMusic + "gameplay_music");
        _collectCoinSound = Content.Load<SoundEffect>(ContentFolderSounds + "collect_coin");
        _collectGasSound = Content.Load<SoundEffect>(ContentFolderSounds + "collect_gas");
        _collectWrenchSound = Content.Load<SoundEffect>(ContentFolderSounds + "collect_wrench");
        _repairSound = Content.Load<SoundEffect>(ContentFolderSounds + "repair");
        _crashLateralSound = Content.Load<SoundEffect>(ContentFolderSounds + "crash_lateral");
        _crashFrontalSound = Content.Load<SoundEffect>(ContentFolderSounds + "crash_frontal");
        _gameOverSound = Content.Load<SoundEffect>(ContentFolderSounds + "game_over");

        // Para que este en loop la musica
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(_menuSong);
        #endregion

        base.LoadContent();
    }

    private void SpawnCollectiblesAndObstacles()
    {
        // Creo collectibles en orden random pero garantizando nafta cada 3 spawns
        int collectibleCounter = 0;
        foreach (var spawnPoint in _collectibleSpawnPoints)
        {
            CollectibleType collectibleType;
            if (collectibleCounter >= 2)
            {
                collectibleType = CollectibleType.Gas;
                collectibleCounter = 0;
            }
            else
            {
                collectibleType = (CollectibleType)_random.Next(0, 3); // 0=Coin, 1=Gas, 2=Wrench
                if (collectibleType == CollectibleType.Gas)
                {
                    collectibleCounter = 0;
                }
                else
                {
                    collectibleCounter++;
                }
            }
            _collectibles.Add(new Collectible(collectibleType, spawnPoint));
        }

        foreach (var spawnPoint in _obstacleSpawnPoints)
        {
            var randomType = (ObstacleType)_random.Next(0, 3); // 0=Cow, 1=Deer, 2=Goat
            _obstacles.Add(new Obstacle(randomType, spawnPoint));
        }
    }

    /// <summary>
    ///     Se llama en cada frame.
    ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
    ///     ante ellas.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        // Aca deberiamos poner toda la logica de actualizacion del juego.
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyboardState = Keyboard.GetState();

        // Capturing pressed keys
        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            //Exit of the game
            Exit();
        }

        // --- Lógica para activar/desactivar el modo Debug ---
        if (keyboardState.IsKeyDown(Keys.F12) && _previousKeyboardState.IsKeyUp(Keys.F12))
        {
            // Si F12 está presionada AHORA pero NO estaba presionada en el frame anterior
            _isDebugModeEnabled = !_isDebugModeEnabled; // (toggle)
        }

        if (_gameOver)
        {
            if (keyboardState.IsKeyDown(Keys.Enter))
            {
                RestartGame();
            }
            base.Update(gameTime);
            return;
        }

        switch (status)
        {
            case ST_PRESENTACION:
                if (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != _menuSong)
                {
                    MediaPlayer.Play(_menuSong);
                }
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    status = ST_SELECCION;
                }
                break;
            case ST_SELECCION:
                if (MediaPlayer.State != MediaState.Playing || MediaPlayer.Queue.ActiveSong != _menuSong)
                {
                    MediaPlayer.Play(_menuSong);
                }

                #region MenuSeleccion
                if (keyboardState.IsKeyDown(Keys.D1)) // F1 Car
                {
                    _selectedCarModel = _f1CarModel;
                    _selectedCarColor = Color.Crimson;
                    // Parámetros para el F1: Alta velocidad, alta aceleración, buen freno, poco drift (mucho agarre)
                    MaxSpeed = 450f;
                    Acceleration = 180f;
                    BrakeDeceleration = 300f;
                    DriftFactor = 0.85f;

                    MediaPlayer.Stop();
                    MediaPlayer.Play(_gameplaySong);
                    status = ST_STAGE_1;
                }
                if (keyboardState.IsKeyDown(Keys.D2)) // Racing Car
                {
                    _selectedCarModel = _racingCarModel;
                    _selectedCarColor = Color.Blue;
                    // Parámetros Equilibrados: Buena velocidad y aceleración, frenado decente, drift moderado
                    MaxSpeed = 350f;
                    Acceleration = 120f;
                    BrakeDeceleration = 200f;
                    DriftFactor = 0.92f;

                    MediaPlayer.Stop();
                    MediaPlayer.Play(_gameplaySong);
                    status = ST_STAGE_1;
                }
                if (keyboardState.IsKeyDown(Keys.D3)) // Cybertruck
                {
                    _selectedCarModel = _cybertruckModel;
                    _selectedCarColor = Color.DarkGray;
                    // Parámetros "Pesados": Menor velocidad y aceleración, frenado pobre, mucho drift (poco agarre)
                    MaxSpeed = 250f;
                    Acceleration = 80f;
                    BrakeDeceleration = 150f;
                    DriftFactor = 0.97f;

                    MediaPlayer.Stop();
                    MediaPlayer.Play(_gameplaySong);
                    status = ST_STAGE_1;
                }
                #endregion
                break;
            case ST_STAGE_1:
                #region Logica del juego

                // Consumo de nafta
                _fuel -= 5f * deltaTime;

                // Reparacion
                if (keyboardState.IsKeyDown(Keys.R) && _wrenches > 0 && _health < 100f)
                {
                    _health = 100f;
                    _wrenches--;
                    _repairSound?.Play();
                }

                #endregion

                #region Movimiento del auto
                // Automatic acceleration
                if (_carSpeed < MaxSpeed)
                    _carSpeed += Acceleration * deltaTime;
                else
                    _carSpeed = MaxSpeed;

                // Braking
                if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Space))
                {
                    _carSpeed -= BrakeDeceleration * deltaTime;
                    if (_carSpeed < 0f) _carSpeed = 0f;
                }

                // Rotation
                float turn = 0f;
                if (keyboardState.IsKeyDown(Keys.A))
                    turn += TurnSpeed * deltaTime;
                if (keyboardState.IsKeyDown(Keys.D))
                    turn -= TurnSpeed * deltaTime;

                _carRotation += turn;

                // Drift effect
                var desiredDirection = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.ToRadians(_carRotation)));
                _carDirection = Vector3.Normalize(_carDirection * DriftFactor + desiredDirection * (1f - DriftFactor));

                // Moving the car
                _carPosition -= _carDirection * _carSpeed * deltaTime;

                if (_selectedCarModel == _f1CarModel)
                {
                    _carWorld = Matrix.CreateScale(0.1f)
                                * Matrix.CreateRotationY(MathHelper.ToRadians(_carRotation))
                                * Matrix.CreateRotationY(MathHelper.ToRadians(90f)) // Orientation correction for the F1 car
                                * Matrix.CreateTranslation(_carPosition);
                }
                else
                {
                    _carWorld = Matrix.CreateScale(0.1f)
                                * Matrix.CreateRotationY(MathHelper.ToRadians(_carRotation))
                                * Matrix.CreateTranslation(_carPosition);
                }

                _carBoundingSphere.Center = _carPosition;
                #endregion

                #region Coleccionables
                // Animacion de los coleccionables
                foreach (var collectible in _collectibles)
                {
                    if (collectible.IsActive)
                    {
                        collectible.Update(gameTime);
                    }
                }

                // Deteccion de colisiones
                foreach (var collectible in _collectibles)
                {
                    if (collectible.IsActive)
                    {
                        if (_carBoundingSphere.Intersects(collectible.BoundingSphere)) // hay colision
                        {
                            collectible.IsActive = false;

                            switch (collectible.Type)
                            {
                                case CollectibleType.Coin:
                                    _score += 100;
                                    _collectCoinSound?.Play();
                                    break;
                                case CollectibleType.Gas:
                                    _fuel = 100f;
                                    _collectGasSound?.Play();
                                    break;
                                case CollectibleType.Wrench:
                                    _wrenches++;
                                    _collectWrenchSound?.Play();
                                    break;
                            }
                        }
                    }
                }
                #endregion

                #region Detección de Colisiones con Obstáculos
                CheckCollisions();
                #endregion

                if (_health <= 0f || _fuel <= 0f && !_gameOver)
                {
                    _gameOver = true;
                    MediaPlayer.Stop();
                    _gameOverSound?.Play();
                }
                break;
        }
        _previousKeyboardState = keyboardState;
        _camera.Update(_carWorld, _carRotation);
        base.Update(gameTime);
    }

    private void CheckCollisions()
    {
        const float FrontalCollisionThreshold = 0.95f; // Umbral para considerar un choque como frontal/trasero

        foreach (var obstacle in _obstacles)
        {
            if (_carBoundingSphere.Intersects(obstacle.BoundingSphere))
            {
                // Vector que va desde el centro del auto hacia el obstáculo
                Vector3 collisionDirection = Vector3.Normalize(obstacle.BoundingSphere.Center - _carBoundingSphere.Center);

                // El producto punto nos dice qué tan alineados están los vectores.
                // Si es cercano a 1 (o -1), el choque es frontal (o trasero).
                // Si es cercano a 0, el choque es lateral.
                float dotProduct = Vector3.Dot(_carDirection, collisionDirection);

                if (Math.Abs(dotProduct) > FrontalCollisionThreshold)
                {
                    // CHOQUE FRONTAL O TRASERO = DAÑO TOTAL
                    _health = 0f;
                    _crashFrontalSound?.Play();
                }
                else
                {
                    // CHOQUE LATERAL = DAÑO PARCIAL
                    _health -= 35f;

                    // Efecto de rebote simple
                    _carSpeed *= 0.5f;
                    _crashLateralSound?.Play();
                }

                break;
            }
        }
    }

    private void RestartGame()
    {
        // Resetear estado del jugador
        _score = 0;
        _fuel = 100f;
        _health = 100f;
        _wrenches = 0;

        // Resetear estado del auto
        _carPosition = new Vector3(0f, 0f, _roadLength - 200f);
        _carRotation = 0f;
        _carSpeed = 0f;
        _carDirection = Vector3.Forward;

        // Limpiar listas de objetos
        _collectibles.Clear();
        _obstacles.Clear();

        // Vuelvo a spawnear objetos
        SpawnCollectiblesAndObstacles();

        // Frenar musica
        MediaPlayer.Stop();
        MediaPlayer.Play(_menuSong);

        // Estado del juego reiniciado a menu
        status = ST_PRESENTACION;
        _gameOver = false;
    }

    /// <summary>
    ///     Se llama cada vez que hay que refrescar la pantalla.
    ///     Escribir aqui el codigo referido al renderizado.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
        var frustum = _camera.Frustum;

        GraphicsDevice.Clear(Color.LightBlue);

        switch (status)
        {
            case ST_PRESENTACION:
                DrawMenu(_menuStart);
                break;
            case ST_SELECCION:
                DrawMenu(_menuSelection);
                break;
            case ST_STAGE_1:

                _basicShader.Parameters["View"].SetValue(_camera.View);
                _basicShader.Parameters["Projection"].SetValue(_camera.Projection);

                ModelDrawingHelper.Draw(_selectedCarModel, _carWorld, _camera.View, _camera.Projection, _selectedCarColor, _basicShader);

                // Dibujar múltiples árboles a ambos lados del camino
                float treeSpacing = 200f; // Espaciado entre árboles
                float treeDistance = 120f; // Distancia desde el centro del camino

                for (float z = -_roadLength + treeSpacing; z < _roadLength; z += treeSpacing)
                {
                    // Árboles del lado derecho (X positivo)
                    Matrix rightTreeWorld = Matrix.CreateScale(20f + (z % 100) / 20f) * // Variación de tamaño de los árboles
                                           Matrix.CreateRotationY(z * 0.01f) * // Rotación de los árboles
                                           Matrix.CreateTranslation(new Vector3(treeDistance, 0f, z));
                    ModelDrawingHelper.Draw(_treeModel, rightTreeWorld, _camera.View, _camera.Projection, Color.Green, _basicShader);

                    // Árboles del lado izquierdo (X negativo)
                    Matrix leftTreeWorld = Matrix.CreateScale(15f + ((z + 50) % 100) / 15f) * // Variación de tamaño de los árboles
                                          Matrix.CreateRotationY((z + 100) * 0.01f) * // Rotación de los árboles
                                          Matrix.CreateTranslation(new Vector3(-treeDistance, 0f, z + 100f)); // Offset para que no estén alineados los árboles
                    ModelDrawingHelper.Draw(_treeModel, leftTreeWorld, _camera.View, _camera.Projection, Color.Green, _basicShader);
                }

                GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
                // Draw the floor
                _grassShader.Parameters["View"].SetValue(_camera.View);
                _grassShader.Parameters["Projection"].SetValue(_camera.Projection);
                _grassShader.Parameters["World"].SetValue(_floorWorld);
                _grassShader.Parameters["Time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

                // tuning
                _grassShader.Parameters["WindSpeed"].SetValue(1.0f);
                _grassShader.Parameters["WindScale"].SetValue(0.12f);
                _grassShader.Parameters["WindStrength"].SetValue(0.6f);
                _grassShader.Parameters["Exposure"].SetValue(1.4f);

                _grassShader.Parameters["Tiling"].SetValue(1000f);          // cuantas repeticiones de la textura
                _grassShader.Parameters["ScrollSpeed"].SetValue(0.0f); // if != 0, the texture will move
                _grassShader.Parameters["TextureInfluence"].SetValue(0.65f);
                _grassShader.Parameters["GrassTexture"].SetValue(_grassTexture);
                _floor.Draw(_grassShader);

                Matrix[] houseWorlds =
                {
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,1.02f,-2500),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300, 1.02f, -2000),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,1.02f,-1500),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300,1.02f,-1000),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,1.02f,-500),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300,1.02f,0),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,1.02f,500),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300,1.02f,1000),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,1.02f,1500),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300,1.02f,2000),
                    Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,1.02f,2500)
                };

                foreach (var world in houseWorlds)
                {
                    ModelDrawingHelper.Draw(_houseModel, world, _camera.View, _camera.Projection, Color.FromNonPremultiplied(61, 38, 29, 255), _basicShader);
                }

                // Dibujar casas en cada posición de empty
                foreach (var world in casasWorld)
                {
                    ModelDrawingHelper.Draw(_houseModel, world, _camera.View, _camera.Projection, Color.FromNonPremultiplied(61, 38, 29, 255), _basicShader);
                }

                foreach (var world in piedrasWorld)
                {
                    ModelDrawingHelper.Draw(_rockModel, world, _camera.View, _camera.Projection, Color.Gray, _basicShader);
                }

                foreach (var world in plantasWorld)
                {
                    ModelDrawingHelper.Draw(_plantModel, world, _camera.View, _camera.Projection, Color.Green, _basicShader);
                }

                _basicShader.Parameters["World"].SetValue(_roadWorld);
                _basicShader.Parameters["UseTexture"].SetValue(1f);
                _basicShader.Parameters["MainTexture"].SetValue(_roadTexture);
                _road.Draw(_basicShader);

                for (float z = -_roadLength + _lineSpacing; z < _roadLength; z += _lineSpacing)
                {
                    _lineWorld = Matrix.CreateScale(_lineWidth, 1f, _lineLength) * Matrix.CreateTranslation(new Vector3(0, 1f, z)); // 0.04 para evitar z-fighting

                    _basicShader.Parameters["World"].SetValue(_lineWorld);
                    _basicShader.Parameters["UseTexture"].SetValue(0f);
                    _basicShader.Parameters["DiffuseColor"].SetValue(Color.Yellow.ToVector3());
                    _line.Draw(_basicShader);
                }


                ModelDrawingHelper.Draw(_trackModel, trackWorld, _camera.View, _camera.Projection, _roadTexture, _basicShader);

                foreach (var collectible in _collectibles)
                {
                    // OPTIMIZACION PARA NO RENDERIZAR LO QUE NO SE ESTA VIENDO
                    if (!frustum.Intersects(collectible.BoundingSphere))
                        continue;

                    if (collectible.IsActive)
                    {
                        switch (collectible.Type)
                        {
                            case CollectibleType.Coin:
                                ModelDrawingHelper.Draw(_coinModel, collectible.World, _camera.View, _camera.Projection, Color.Gold, _basicShader);
                                break;
                            case CollectibleType.Gas:
                                ModelDrawingHelper.Draw(_gasModel, collectible.World, _camera.View, _camera.Projection, Color.DarkRed, _basicShader);
                                break;
                            case CollectibleType.Wrench:
                                ModelDrawingHelper.Draw(_wrenchModel, collectible.World, _camera.View, _camera.Projection, Color.LightGray, _basicShader);
                                break;
                        }
                    }
                }

                #region Obstaculos
                foreach (var obstacle in _obstacles)
                {
                    if (!frustum.Intersects(obstacle.BoundingSphere))
                        continue;

                    switch (obstacle.Type)
                    {
                        case ObstacleType.Cow:
                            ModelDrawingHelper.Draw(_cowModel, obstacle.World, _camera.View, _camera.Projection, Color.White, _basicShader);
                            break;
                        case ObstacleType.Deer:
                            ModelDrawingHelper.Draw(_deerModel, obstacle.World, _camera.View, _camera.Projection, Color.SaddleBrown, _basicShader);
                            break;
                        case ObstacleType.Goat:
                            ModelDrawingHelper.Draw(_goatModel, obstacle.World, _camera.View, _camera.Projection, Color.LightGray, _basicShader);
                            break;
                    }
                }
                #endregion

                #region Modo debug
                if (_isDebugModeEnabled)
                {
                    #region Dibujado de Esferas de Debug
                    GraphicsDevice.RasterizerState = _wireframeRasterizerState;

                    // Dibujar la esfera del auto
                    var carSphereWorld = Matrix.CreateScale(_carBoundingSphere.Radius) * Matrix.CreateTranslation(_carBoundingSphere.Center);
                    ModelDrawingHelper.Draw(_debugSphereModel, carSphereWorld, _camera.View, _camera.Projection, Color.Blue, _basicShader);

                    // Dibujar la esfera de cada coleccionable activo
                    foreach (var collectible in _collectibles)
                    {
                        if (collectible.IsActive)
                        {
                            var collectibleSphereWorld = Matrix.CreateScale(collectible.BoundingSphere.Radius) * Matrix.CreateTranslation(collectible.BoundingSphere.Center);
                            ModelDrawingHelper.Draw(_debugSphereModel, collectibleSphereWorld, _camera.View, _camera.Projection, Color.Yellow, _basicShader);
                        }
                    }

                    // Dibujar la esfera de cada obstáculo
                    foreach (var obstacle in _obstacles)
                    {
                        var obstacleSphereWorld = Matrix.CreateScale(obstacle.BoundingSphere.Radius) * Matrix.CreateTranslation(obstacle.BoundingSphere.Center);
                        ModelDrawingHelper.Draw(_debugSphereModel, obstacleSphereWorld, _camera.View, _camera.Projection, Color.Red, _basicShader);
                    }

                    GraphicsDevice.RasterizerState = _solidRasterizerState;
                    #endregion
                }
                #endregion

                #region Dibujado del HUD
                _spriteBatch.Begin();

                if (_gameOver)
                {
                    var gameOverText = "GAME OVER";
                    var restartText = "Press Enter to Restart";

                    var gameOverSize = _gameOverFont.MeasureString(gameOverText);
                    var restartSize = _mainFont.MeasureString(restartText);

                    var screenCenter = GraphicsDevice.Viewport.Bounds.Center.ToVector2();

                    var gameOverPosition = screenCenter - new Vector2(gameOverSize.X / 2, gameOverSize.Y);
                    var restartPosition = screenCenter - new Vector2(restartSize.X / 2, -10f);

                    _spriteBatch.DrawString(_gameOverFont, gameOverText, gameOverPosition, Color.Red);
                    _spriteBatch.DrawString(_mainFont, restartText, restartPosition, Color.White);
                }
                else
                {
                    // --- HUD DURANTE EL JUEGO ---
                    var startX = 50;
                    var currentY = 50;
                    var textMargin = 10;
                    var elementSpacing = 40;
                    var iconSize = 32;
                    var textColor = Color.White;

                    // Score
                    var scoreIconPosition = new Vector2(startX, currentY);
                    _spriteBatch.Draw(_coinIcon, new Rectangle((int)scoreIconPosition.X, (int)scoreIconPosition.Y, iconSize, iconSize), Color.White);

                    var scoreTextPosition = new Vector2(startX + iconSize + textMargin, currentY + (iconSize - _mainFont.MeasureString("Score:").Y) / 2);
                    _spriteBatch.DrawString(_mainFont, "Score: " + _score, scoreTextPosition, textColor);

                    currentY += elementSpacing;

                    // Repairs
                    var wrenchIconPosition = new Vector2(startX, currentY);
                    _spriteBatch.Draw(_wrenchIcon, new Rectangle((int)wrenchIconPosition.X, (int)wrenchIconPosition.Y, iconSize, iconSize), Color.White);

                    var wrenchesText = "Repairs: " + _wrenches;
                    var wrenchesTextSize = _mainFont.MeasureString(wrenchesText);
                    var wrenchesTextPosition = new Vector2(startX + iconSize + textMargin, currentY + (iconSize - wrenchesTextSize.Y) / 2);
                    _spriteBatch.DrawString(_mainFont, wrenchesText, wrenchesTextPosition, textColor);

                    // AÑADIMOS EL TEXTO DE AYUDA PARA REPARAR
                    var repairHelpText = "(Press R to use)";
                    var repairHelpTextPosition = new Vector2(wrenchesTextPosition.X + wrenchesTextSize.X + textMargin, wrenchesTextPosition.Y);
                    _spriteBatch.DrawString(_mainFont, repairHelpText, repairHelpTextPosition, Color.LightGray);

                    currentY += elementSpacing;

                    // Barra de Vida (health)
                    _spriteBatch.DrawString(_mainFont, "health", new Vector2(startX, currentY), textColor);
                    currentY += 25;

                    var healthBarPosition = new Vector2(startX, currentY);
                    var barWidth = 200;
                    var barHeight = 20;

                    var healthBackgroundRect = new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y, barWidth, barHeight);
                    var healthRect = new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y, (int)(barWidth * (_health / 100f)), barHeight);

                    Color healthColor = Color.Lerp(Color.Red, Color.LightGreen, _health / 100f);

                    _spriteBatch.Draw(_pixelTexture, healthBackgroundRect, Color.DarkGray);
                    _spriteBatch.Draw(_pixelTexture, healthRect, healthColor);

                    currentY += elementSpacing;

                    // Fuel
                    var gasIconPosition = new Vector2(startX, currentY);
                    _spriteBatch.Draw(_gasIcon, new Rectangle((int)gasIconPosition.X, (int)gasIconPosition.Y, iconSize, iconSize), Color.White);

                    var fuelLabelTextPosition = new Vector2(startX + iconSize + textMargin, currentY + (iconSize - _mainFont.MeasureString("Fuel").Y) / 2);
                    _spriteBatch.DrawString(_mainFont, "Fuel", fuelLabelTextPosition, textColor);

                    currentY += iconSize + 5;

                    var fuelBarPosition = new Vector2(startX, currentY);
                    var fuelBackgroundRect = new Rectangle((int)fuelBarPosition.X, (int)fuelBarPosition.Y, barWidth, barHeight);
                    var fuelRect = new Rectangle((int)fuelBarPosition.X, (int)fuelBarPosition.Y, (int)(barWidth * (_fuel / 100f)), barHeight);

                    _spriteBatch.Draw(_pixelTexture, fuelBackgroundRect, Color.DarkGray);
                    _spriteBatch.Draw(_pixelTexture, fuelRect, Color.Green);
                }

                _spriteBatch.End();
                #endregion

                // IMPORTANTE: Restaurar estados de renderizado que SpriteBatch modifica
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                break;
        }
        base.Draw(gameTime);
    }

    /// <summary>
    ///     Libero los recursos que se cargaron en el juego.
    /// </summary>
    protected override void UnloadContent()
    {
        // Libero los recursos.
        Content.Unload();

        base.UnloadContent();
    }

    public void DrawMenu(Texture2D menu)
    {
        _spriteBatch.Begin();
        _spriteBatch.Draw(menu, new Rectangle(0, 0, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100), Color.White);
        _spriteBatch.End();
    }
}