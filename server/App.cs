using Stormancer;
using Stormancer.Core;
using Stormancer.Server;
using System.Collections.Generic;


namespace Test
{
    public class Startup
    {
        public void Build(IAppBuilder builder)
        {
            builder.SceneTemplate("test-template", scene =>
            {
                scene.AddRoute("echo.in", p =>
                {
                    scene.Broadcast("echo.out", s => p.Stream.CopyTo(s), Core.PacketPriority.MEDIUM_PRIORITY, Core.PacketReliability.RELIABLE);
                });
            },
            new Dictionary<string, string> { { "description", "Broadcasts data sent to the route 'echo.in' to all connected users on the route 'echo.out'." } });
        }
    }
}