using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Continuum.Nonlinear;

public static class DynamicResidualBuilder
{
    public static double[] Build(
        NonlinearAssemblyResult ar,
        SparseCsrMatrix mass,
        SparseCsrMatrix damping,
        double[] x,
        double[] predictor,
        double[] velocityPredictor,
        double[] external,
        double dt,
        double beta,
        double gamma
    )
    {
        var acc = new double[x.Length];
        var vel = new double[x.Length];
        var a0 = 1 / (beta * dt * dt);
        for (var i = 0; i < x.Length; i++)
        {
            acc[i] = (x[i] - predictor[i]) * a0;
            vel[i] = velocityPredictor[i] + gamma * dt * acc[i];
        }
        var ma = SparseMatrixAlgebra.Multiply(mass, acc);
        var cv = SparseMatrixAlgebra.Multiply(damping, vel);
        var r = new double[x.Length];
        for (var i = 0; i < r.Length; i++)
            r[i] = ar.InternalForce[i] + ma[i] + cv[i] - external[i];
        return r;
    }
}
