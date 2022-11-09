using System.Numerics;
using TA1;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace ImGuiNET
{
    static class Program
    {
        //System variables
        private static Sdl2Window? _window;
        private static GraphicsDevice? _gd;
        private static CommandList? _cl;
        private static ImGuiController? _controller;
        //Program variables
        private static Graph? _graph;
        private static Inspector? _inspector;
        private static IterationMode? _iterationMode;
        private static TableOfNodes? _tableOfNodes;
        private static LSAwindow? _lsaWindow;
        private static Dockspace? _dockspace;

        // UI state
        private static readonly Vector3 ClearColor = new Vector3(0.45f, 0.55f, 0.6f);

        static void Main()
        {
            // Create window, GraphicsDevice, and all resources necessary for the demo.
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1920, 1080, WindowState.Normal, "ImGui.NET Sample Program"),
                new GraphicsDeviceOptions(true, null, true, ResourceBindingModel.Improved, false, true),
                out _window,
                out _gd);
            _window.Resized += () =>
            {
                _gd?.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
                _controller?.WindowResized(_window.Width, _window.Height);
                Graph.SetPaintArea(new Vector2(_window.Width,_window.Height));
            };
            _window.KeyDown += key =>
            {
                if(key.Key == Key.Escape)
                    _window.Close();
                if(key.Key == Key.F5)
                    _graph?.ReloadEdges();
            };
            _cl = _gd.ResourceFactory.CreateCommandList();
            _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);

            //Initialization of TA objects
            InitVariables();

            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.ViewportsEnable | ImGuiConfigFlags.DockingEnable;
            
            // Main application loop
            while (_window.Exists)
            {
                var snapshot = _window.PumpEvents();
                if (!_window.Exists) { break; }
                _controller.Update(1f / 60f, snapshot); // Feed the input events to our ImGui controller, which passes them through to ImGui.

                ImGuiUi();

                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(ClearColor.X, ClearColor.Y, ClearColor.Z, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }

        private static void InitVariables()
        {
            _graph = new Graph();
            _lsaWindow = new();
            _iterationMode = new IterationMode(_graph);
            _tableOfNodes = new TableOfNodes(_graph);
            _inspector = new Inspector{Graph = _graph};
            _dockspace = new Dockspace();
        }
        
        //Main loop of program
        private static void ImGuiUi()
        {
            _dockspace?.Draw();
            _graph?.Draw();
            _inspector?.Draw();
            if (_graph != null) _lsaWindow?.Draw(ref _graph);
            _iterationMode?.Draw();
            _tableOfNodes?.Draw();
        }
    }
}
