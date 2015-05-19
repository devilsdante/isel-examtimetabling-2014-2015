namespace DAL.Models.Solution
{
    public interface ISolution : IEntity
    {
        int fitness { get; set; }
        ISolution Copy();
        bool Equals(ISolution solution);
    }
}
