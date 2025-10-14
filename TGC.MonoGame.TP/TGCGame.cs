using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TGC.MonoGame.TP.Zero;

namespace TGC.MonoGame.TP;

/// <summary>
///     Esta es la clase principal del juego.
///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
/// </summary>
public class TGCGame : Game
{
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
    private float _fuel = 100f; // El combustible empieza al máximo
    private int _wrenches = 0;
    private SpriteFont _mainFont;
    private Texture2D _coinIcon;
    private Texture2D _wrenchIcon;
    private Texture2D _gasIcon;
    private Texture2D _pixelTexture;
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
    public const int ST_PRESENTACION = 0; //Menu Principal (no implementado)
    public const int ST_SELECCION = 1;
    public const int ST_STAGE_1 = 2;
    public int status = ST_SELECCION;
    private Texture2D _menuSeleccion;

    #endregion
    private readonly GraphicsDeviceManager _graphics;
    private Camera _camera;
    private SpriteBatch _spriteBatch;

    private List<Vector3> _collectibleSpawnPoints = new List<Vector3>();
    private List<Collectible> _collectibles = new List<Collectible>();

    public enum TerrainType
    {
        Asphalt = 0,
        Dirt = 1,
        Snow = 2
    }

    private TerrainType _currentTerrain;
    private Random _random = new Random();

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

        // Apago el backface culling.
        // Esto se hace por un problema en el diseno del modelo del logo de la materia.
        // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
        // var rasterizerState = new RasterizerState();
        // rasterizerState.CullMode = CullMode.None;
        // GraphicsDevice.RasterizerState = rasterizerState;
        // Seria hasta aca.

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
        _carPosition = new Vector3(0f, 0f, _roadLength);

        trackWorld = Matrix.CreateScale(0.66f) * Matrix.CreateRotationY(-MathHelper.PiOver2)
                                                      * Matrix.CreateTranslation(-455, 1f, 3200);

        _currentTerrain = (TerrainType)_random.Next(0, 3);

        _carBoundingSphere.Center = _carPosition;
        _carBoundingSphere.Radius = CarBoundingSphereRadius;

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
        _coinIcon = Content.Load<Texture2D>(ContentFolderTextures + "HUD/coin_icon");
        _wrenchIcon = Content.Load<Texture2D>(ContentFolderTextures + "HUD/wrench_icon");
        _gasIcon = Content.Load<Texture2D>(ContentFolderTextures + "HUD/gas_icon");
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        #endregion

        _menuSeleccion = Content.Load<Texture2D>("Menus/menu_vehicle_selection");
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

        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        _basicShader = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
        _grassShader = Content.Load<Effect>(ContentFolderEffects + "GrassShader");

        switch (_currentTerrain)
        {
            case TerrainType.Asphalt:
                // Load asphalt textures
                _grassTexture = Content.Load<Texture2D>(ContentFolderTextures + "Asphalt/OffRoad/grassTextureV2");
                _roadTexture = Content.Load<Texture2D>(ContentFolderTextures + "Asphalt/Road/asphaltColor");
                MaxSpeed = 300f;
                Acceleration = 100f;
                BrakeDeceleration = 250f;
                DriftFactor = 0.75f;
                break;
            case TerrainType.Dirt:
                // Load dirt textures
                _grassTexture = Content.Load<Texture2D>(ContentFolderTextures + "Dirt/OffRoad/grassTextureV2");
                _roadTexture = Content.Load<Texture2D>(ContentFolderTextures + "Dirt/Road/dirtTexture");
                MaxSpeed = 200f;
                Acceleration = 80f;
                BrakeDeceleration = 150f;
                DriftFactor = 0.96f;
                break;
            case TerrainType.Snow:
                // Load snow textures
                _grassTexture = Content.Load<Texture2D>(ContentFolderTextures + "Snow/OffRoad/snowColor");
                _roadTexture = Content.Load<Texture2D>(ContentFolderTextures + "Snow/Road/snowDirtColor");
                MaxSpeed = 180f;
                Acceleration = 70f;
                BrakeDeceleration = 100f;
                DriftFactor = 0.97f;
                break;
        }

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

        _floor = new QuadPrimitive(GraphicsDevice);
        _road = new QuadPrimitive(GraphicsDevice);
        _line = new QuadPrimitive(GraphicsDevice);

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
        }

        // Creo collectibles en orden random
        foreach (var spawnPoint in _collectibleSpawnPoints)
        {
            var randomType = (CollectibleType)_random.Next(0, 3); // 0=Coin, 1=Gas, 2=Wrench
            _collectibles.Add(new Collectible(randomType, spawnPoint));
        }

        base.LoadContent();
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

        switch (status)
        {
            case ST_SELECCION:
                #region MenuSeleccion
                if (keyboardState.IsKeyDown(Keys.D1))
                {
                    _selectedCarModel = _f1CarModel;
                    status = ST_STAGE_1;
                }
                if (keyboardState.IsKeyDown(Keys.D2))
                {
                    _selectedCarModel = _racingCarModel;
                    status = ST_STAGE_1;
                }
                if (keyboardState.IsKeyDown(Keys.D3))
                {
                    _selectedCarModel = _cybertruckModel;
                    status = ST_STAGE_1;
                }
                #endregion
                break;
            case ST_STAGE_1:
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

                _carWorld = Matrix.CreateScale(0.1f) * Matrix.CreateRotationY(MathHelper.ToRadians(_carRotation))
                            * Matrix.CreateTranslation(_carPosition);

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
                                    break;
                                case CollectibleType.Gas:
                                    _fuel = 100f;
                                    break;
                                case CollectibleType.Wrench:
                                    _wrenches++;
                                    break;
                            }
                        }
                    }
                }
                #endregion

                break;
        }
        _camera.Update(_carWorld, _carRotation);
        base.Update(gameTime);
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
            case ST_SELECCION:
                DrawSelectionMenu();
                break;
            case ST_STAGE_1:

                _basicShader.Parameters["View"].SetValue(_camera.View);
                _basicShader.Parameters["Projection"].SetValue(_camera.Projection);

                ModelDrawingHelper.Draw(_selectedCarModel, _carWorld, _camera.View, _camera.Projection, Color.Red, _basicShader);

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
                                ModelDrawingHelper.Draw(_gasModel, collectible.World, _camera.View, _camera.Projection, Color.Red, _basicShader);
                                break;
                            case CollectibleType.Wrench:
                                ModelDrawingHelper.Draw(_wrenchModel, collectible.World, _camera.View, _camera.Projection, Color.LightGray, _basicShader);
                                break;
                        }
                    }
                }

                #region Dibujado del HUD

                _spriteBatch.Begin();

                // --- Definiciones Generales para el HUD ---
                var startX = 50;
                var currentY = 50;
                var textMargin = 10;
                var elementSpacing = 40;
                var iconSize = 40;
                var textColor = Color.White;

                // --- 1. Score (Puntaje) ---
                var scoreIconPosition = new Vector2(startX, currentY);
                _spriteBatch.Draw(_coinIcon, new Rectangle((int)scoreIconPosition.X, (int)scoreIconPosition.Y, iconSize, iconSize), Color.White);

                var scoreTextPosition = new Vector2(startX + iconSize + textMargin, currentY + (iconSize - _mainFont.MeasureString("Score:").Y) / 2);
                _spriteBatch.DrawString(_mainFont, "Score: " + _score, scoreTextPosition, textColor);

                currentY += elementSpacing;

                // --- 2. Repairs (Llaves) ---
                var wrenchIconPosition = new Vector2(startX, currentY);
                _spriteBatch.Draw(_wrenchIcon, new Rectangle((int)wrenchIconPosition.X, (int)wrenchIconPosition.Y, iconSize, iconSize), Color.White);

                var wrenchesTextPosition = new Vector2(startX + iconSize + textMargin, currentY + (iconSize - _mainFont.MeasureString("Repairs:").Y) / 2);
                _spriteBatch.DrawString(_mainFont, "Repairs: " + _wrenches, wrenchesTextPosition, textColor);

                currentY += elementSpacing;

                // --- 3. Fuel (Combustible) ---
                var gasIconPosition = new Vector2(startX, currentY);
                _spriteBatch.Draw(_gasIcon, new Rectangle((int)gasIconPosition.X, (int)gasIconPosition.Y, iconSize, iconSize), Color.White);

                var fuelLabelTextPosition = new Vector2(startX + iconSize + textMargin, currentY + (iconSize - _mainFont.MeasureString("Fuel").Y) / 2);
                _spriteBatch.DrawString(_mainFont, "Fuel", fuelLabelTextPosition, textColor);

                currentY += iconSize + 5;

                // Lógica para la barra de combustible
                var fuelBarPosition = new Vector2(startX, currentY);
                var fuelBarWidth = 200;
                var fuelBarHeight = 20;

                var backgroundRect = new Rectangle((int)fuelBarPosition.X, (int)fuelBarPosition.Y, fuelBarWidth, fuelBarHeight);
                var fuelRect = new Rectangle((int)fuelBarPosition.X, (int)fuelBarPosition.Y, (int)(fuelBarWidth * (_fuel / 100f)), fuelBarHeight);

                // USAMOS LA TEXTURA PRE-CARGADA
                _spriteBatch.Draw(_pixelTexture, backgroundRect, Color.DarkGray);
                _spriteBatch.Draw(_pixelTexture, fuelRect, Color.Green);

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

    public void DrawSelectionMenu()
    {
        _spriteBatch.Begin();
        _spriteBatch.Draw(_menuSeleccion, new Rectangle(0, 0, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100), Color.White);
        _spriteBatch.End();
    }
}