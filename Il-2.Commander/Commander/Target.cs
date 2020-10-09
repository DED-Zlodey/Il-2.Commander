using Il_2.Commander.Data;

namespace Il_2.Commander.Commander
{
    class Target
    {
        public ServerInputs TargetOn { get; set; }
        public ServerInputs TargetOff { get; set; }
        public ServerInputs IconOn { get; set; }
        public ServerInputs IconOff { get; set; }
        public bool Destroyed { get; set; } = false;
        public int TotalW { get; set; }
    }
}
