# Mobility decision-support sample

The WPF **Mobility** workspace exports a numeric CSV and adjacent `*.schema.json` file. TensorFlow can load the CSV directly:

```powershell
python -m pip install tensorflow
python samples/mobility/train_tensorflow.py path/to/traversability.csv
```

The example deliberately trains only on raw vehicle/terrain inputs. Derived utilization and cost columns are excluded to avoid direct rule leakage.

Synthetic labels reproduce `TraversabilityAssessor`; they are pipeline bootstrap data, not independent truth. A deployable model needs measured outcomes, explicit train/validation/test splits by site and vehicle, uncertainty calibration, out-of-distribution detection, and a deterministic safety fallback that can reject low-confidence predictions.
