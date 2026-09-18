namespace ImpactLab.Core.Continuum.Contact;

public readonly record struct ContactConstraintKey(
    int SlaveEntity,
    int MasterEntity,
    int QuadratureIndex
);
