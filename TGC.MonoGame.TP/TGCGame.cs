using System;
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
    private Effect _effect;
    private Model _carModel;
    private Camera _camera;
    private SpriteBatch _spriteBatch;
    private Matrix _carWorld;
    private Matrix _view;
    private Matrix _projection;
    private Vector3 _carPosition = Vector3.Zero;
    private float _carRotation = 0f;
    
    /// <summary>
    /// Geometry to draw a floor
    /// </summary>
    private QuadPrimitive _floor;
    
    /// <summary>
    /// The world matrix for the floor
    /// </summary>
    private Matrix _floorWorld;

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

        // Configuramos nuestras matrices de la escena.
        _carWorld = Matrix.Identity;
        _view = Matrix.Identity;
        _projection = Matrix.Identity;
        
        _floorWorld = Matrix.CreateScale(3000f) * Matrix.CreateTranslation(0f, 0f, 0f);

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

        _camera = new Camera(GraphicsDevice.Viewport.AspectRatio, 1000f, 350f, 50f);

        // Cargo un efecto basico propio declarado en el Content pipeline.
        // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
        _effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

        // Asigno el efecto que cargue a cada parte del mesh.
        // Un modelo puede tener mas de 1 mesh internamente.
        foreach (var mesh in _carModel.Meshes)
        {
            // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            foreach (var meshPart in mesh.MeshParts)
            {
                meshPart.Effect = _effect;
            }
        }

        _floor = new QuadPrimitive(GraphicsDevice);
        
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

        // Capturar Input teclado
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            //Salgo del juego.
            Exit();
        }

        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            _carPosition -= _carWorld.Forward * 20f;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            _carPosition += _carWorld.Forward * 20f;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            _carRotation += 2f;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            _carRotation -= 2f;
        }
        
        _carWorld = Matrix.CreateRotationY(MathHelper.ToRadians(_carRotation)) * Matrix.CreateScale(0.1f) 
                    * Matrix.CreateTranslation(_carPosition);

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
        
        var viewProjection = _camera.View * _camera.Projection;

        // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
        _effect.Parameters["View"].SetValue(_camera.View);
        _effect.Parameters["Projection"].SetValue(_camera.Projection);
        _effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector3());

        foreach (var mesh in _carModel.Meshes)
        {
            _effect.Parameters["World"].SetValue(mesh.ParentBone.Transform * _carWorld);
            _effect.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector3());
            mesh.Draw();
        }
        
        // Draw the floor, pass the World, WorldViewProjection and InverseTransposeWorld matrices
        _effect.Parameters["World"].SetValue(_floorWorld);
        _effect.Parameters["View"].SetValue(_camera.View);
        _effect.Parameters["Projection"].SetValue(_camera.Projection);
        _effect.Parameters["DiffuseColor"].SetValue(Color.Green.ToVector3());
        //_effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Invert(Matrix.Transpose(_floorWorld)));
        //_effect.Parameters["WorldViewProjection"].SetValue(_floorWorld * viewProjection);

        _floor.Draw(_effect);
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