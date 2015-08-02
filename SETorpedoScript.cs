double angle_t0, angle_t1;
double dist_t0, dist_t1;
double t0,t1;
double alpha, v;
double vm = 104.4;

void Main(string argument)
{
    var rotor = GridTerminalSystem.GetBlockWithName("TorpedoRotor") as IMyMotorStator;

    string[] args = argument.Split(':');
    switch(args[0])
    {
        case "a0":
            angle_t0 = fix_angle(rotor.Angle);
            t0 = DateTime.Now.Ticks / 10000000.0;
            break;
        case "a1":
            angle_t1 = fix_angle(rotor.Angle);
            t1 = curtime();
            break;
        case "d0":
            dist_t0 = double.Parse(args[1]);
            break;
        case "d1":
            dist_t1 = double.Parse(args[1]);
            break;
        case "fire":
            fire();
            break;
    }

}

double fix_angle(double angle)
{
    if(angle > Math.PI) return 2*Math.PI - angle;
    return angle;
}

double curtime()
{
    return (DateTime.Now.Ticks / 10000000.0)- t0;
}

double s(double t)
{
    return v*t;
}

double d(double t)
{
    return Math.Sqrt(dist_t0*dist_t0+s(t)*s(t)-2*dist_t0*s(t)*Math.Cos(alpha));
}

double beta(double t)
{
    return Math.Acos((dist_t0*dist_t0+d(t)*d(t)-s(t)*s(t))/(2*dist_t0*d(t)));
}

void calc_data()
{
    v = Math.Sqrt(dist_t0*dist_t0+dist_t1*dist_t1-2*dist_t0*dist_t1*Math.Cos(Math.Abs(angle_t1-angle_t0))) / t1;
    alpha = Math.Acos((dist_t0*dist_t0+s(t1)*s(t1)-dist_t1*dist_t1)/(2*dist_t0*s(t1)));
}

void fire()
{
    calc_data();
    var launch = GridTerminalSystem.GetBlockWithName("Launch") as IMyTimerBlock;
    var deploydecoy = GridTerminalSystem.GetBlockWithName("DeployDecoy") as IMyTimerBlock;

    double dt = (Math.Sin(alpha)*dist_t0/Math.Sin(Math.PI-alpha-angle_t0));
    double t = Math.Sqrt(dist_t0*dist_t0+dt*dt-2*Math.Cos(angle_t0)*dist_t0*dt) / v;
    double tr = dt / vm;
    launch.SetValueFloat("TriggerDelay", (float)(t-tr-curtime()));
    //This line was to program the missile to launch its decoy when in turret range
    //deploydecoy.SetValueFloat("TriggerDelay", (float)((d(t) - 800)/vm));
    launch.GetActionWithName("Start").Apply(launch);
}