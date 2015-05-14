namespace Heuristics.SimulatedAnnealing
{
    public interface ICoolingSchedule
    {
        double TMax { get; set; }
        double TMin { get; set; }
        double rate { get; set; }
        int span { get; set; }

        double G(double T);
    }
}
