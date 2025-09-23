using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Zero;

namespace TGC.MonoGame.TP;

/// <summary>
///     Esta es la clase principal del juego.
///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
/// </summary>
public class TGCGame : Game
{
    public const string ContentFolder3D = "Models/";
    public const string ContentFolderEffects = "Effects/";
    public const string ContentFolderMusic = "Music/";
    public const string ContentFolderSounds = "Sounds/";
    public const string ContentFolderSpriteFonts = "SpriteFonts/";
    public const string ContentFolderTextures = "Textures/";

    private readonly GraphicsDeviceManager _graphics;
    private Effect _basicShader;
    private Effect _grassShader;
    private Texture _grassTexture;
    private Model _carModel;
    private Model _houseModel;
    private Camera _camera;
    private SpriteBatch _spriteBatch;
    private Matrix _carWorld;
    private Vector3 _carPosition;
    private float _carRotation = 0f;

    private Model _treeModel;
    private Model _trackModel;

    // Car's movement variables (need to be adjusted)
    private float _carSpeed = 0f;
    private const float MaxSpeed = 300f;
    private const float Acceleration = 100f;
    private const float BrakeDeceleration = 200f;
    private const float TurnSpeed = 60f;
    private const float DriftFactor = 0.95f; // 0 (no drift) > DriftFactor > 1 (no adhesion)
    private Vector3 _carDirection = Vector3.Forward;

    /// <summary>
    /// Geometry to draw a floor
    /// </summary>
    private QuadPrimitive _floor;

    /// <summary>
    /// The world matrix for the floor
    /// </summary>
    private Matrix _floorWorld;

    private QuadPrimitive _road;
    private Matrix _roadWorld;
    private float _roadLength;
    private float _roadWidth;
    private QuadPrimitive _line;
    private Matrix _lineWorld;
    private float _lineSpacing;
    private float _lineLength;
    private float _lineWidth;

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
        _camera = new Camera(GraphicsDevice.Viewport.AspectRatio, 1500f, 800f, 50f);

        // Configuramos nuestras matrices de la escena.
        _carWorld = Matrix.Identity;

        _floorWorld = Matrix.CreateScale(30000f) * Matrix.CreateTranslation(0f, 0f, 0f);

        // Configuro la ruta
        _roadLength = 3000f;
        _roadWidth = 70f;
        _lineSpacing = 100f;
        _lineLength = 30f;
        _lineWidth = 5f;

        _roadWorld = Matrix.CreateScale(_roadWidth, 1f, _roadLength) * Matrix.CreateTranslation(new Vector3(0f, 0.5f, 0.00f)); // 0.02 para evitar z-fighting

        // Inicializo el auto en el principio de la ruta mas un pequeño offset para que solo se vea el mapa
        _carPosition = new Vector3(0f, 0f, -_roadLength + 100f);

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

        _carModel = Content.Load<Model>(ContentFolder3D + "RacingCarA/RacingCar");
        _treeModel = Content.Load<Model>(ContentFolder3D + "Tree/Tree");
        _houseModel = Content.Load<Model>(ContentFolder3D + "Houses/Cabin");
        _trackModel =  Content.Load<Model>(ContentFolder3D + "Track/pista");

        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        _basicShader = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
        _grassShader = Content.Load<Effect>(ContentFolderEffects + "GrassShader");
        _grassTexture = Content.Load<Texture2D>(ContentFolderTextures + "grassTexture");

        // Asigno el efecto que cargue a cada parte del mesh.
        ModelDrawingHelper.AttachEffectToModel(_carModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_treeModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_houseModel, _basicShader);
        ModelDrawingHelper.AttachEffectToModel(_trackModel, _basicShader);

        _floor = new QuadPrimitive(GraphicsDevice);

        _road = new QuadPrimitive(GraphicsDevice);
        _line = new QuadPrimitive(GraphicsDevice);

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

        #region Movimiento del auto
        // Automatic acceleration
        if (_carSpeed < MaxSpeed)
            _carSpeed += Acceleration * deltaTime;
        else
            _carSpeed = MaxSpeed;

        // Braking
        if (keyboardState.IsKeyDown(Keys.S))
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
        #endregion

        _camera.Update(_carWorld);

        base.Update(gameTime);
    }

    /// <summary>
    ///     Se llama cada vez que hay que refrescar la pantalla.
    ///     Escribir aqui el codigo referido al renderizado.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
        // Aca deberiamos poner toda la logia de renderizado del juego.
        GraphicsDevice.Clear(Color.LightBlue);

        // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
        //! NOTA: SOLO HACE FALTA DEFINIR VIEW Y PROJECTION UNA VEZ
        _basicShader.Parameters["View"].SetValue(_camera.View);
        _basicShader.Parameters["Projection"].SetValue(_camera.Projection);

        ModelDrawingHelper.Draw(_carModel, _carWorld, _camera.View, _camera.Projection, Color.Red, _basicShader);

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

        _grassShader.Parameters["Tiling"].SetValue(200f);          // cuantas repeticiones de la textura
        _grassShader.Parameters["ScrollSpeed"].SetValue(0.02f);
        _grassShader.Parameters["TextureInfluence"].SetValue(0.65f);
        _grassShader.Parameters["GrassTexture"].SetValue(_grassTexture);
        _floor.Draw(_grassShader);
        
        Matrix[] houseWorlds =
        {
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,0,-2500),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300, 0, -2000),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,0,-1500),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300,0,-1000),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,0,-500),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300,0,0),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,0,500),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300,0,1000),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,0,1500),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(-300,0,2000),
            Matrix.CreateScale(0.4f) * Matrix.CreateTranslation(300,0,2500)
        };

        foreach (var world in houseWorlds)
        {
            ModelDrawingHelper.Draw(_houseModel, world, _camera.View, _camera.Projection, Color.Gray, _basicShader);
        }
        
        
        _basicShader.Parameters["World"].SetValue(_roadWorld);
        _basicShader.Parameters["DiffuseColor"].SetValue(Color.Black.ToVector3());
        _road.Draw(_basicShader);
        
        for (float z = -_roadLength + _lineSpacing; z < _roadLength; z += _lineSpacing)
        {
            _lineWorld = Matrix.CreateScale(_lineWidth, 1f, _lineLength) * Matrix.CreateTranslation(new Vector3(0, 1f, z)); // 0.04 para evitar z-fighting

            _basicShader.Parameters["World"].SetValue(_lineWorld);
            _basicShader.Parameters["DiffuseColor"].SetValue(Color.Yellow.ToVector3());
            _line.Draw(_basicShader);
        }
        
        Matrix trackWorld = Matrix.CreateScale(0.66f) * Matrix.CreateRotationY(-MathHelper.PiOver2) 
                                                     * Matrix.CreateTranslation(-455, 1f, 3200);
        ModelDrawingHelper.Draw(_trackModel, trackWorld, _camera.View, _camera.Projection, Color.Black, _basicShader);

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
}