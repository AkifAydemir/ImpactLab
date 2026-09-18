namespace ImpactLab.Core.Continuum.Contact;

public static class MortarShapeFunctions
{
    public static (double N1, double N2, double N3) Triangle(double xi, double eta) =>
        (1 - xi - eta, xi, eta);
}
