using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Contact;

public static class StickSlipJacobian
{
    public static void AddDiagonal(
        SparseTripletBuilder t,
        int node,
        double normalPenalty,
        double tangentPenalty,
        bool sticking
    )
    {
        for (var d = 0; d < 3; d++)
            t.Add(
                node * 3 + d,
                node * 3 + d,
                d == 2 ? normalPenalty : (sticking ? tangentPenalty : 0)
            );
    }
}
