#!/usr/bin/env python3
"""Train a small traversability classifier from an ImpactLab CSV export.

Synthetic ImpactLab labels are useful for pipeline bootstrapping, not deployment
validation. Replace or augment them with measured vehicle/terrain outcomes.
"""

from __future__ import annotations

import argparse
from pathlib import Path

import tensorflow as tf


FEATURES = [
    "vehicle_mass_kg",
    "contact_area_m2",
    "ground_clearance_m",
    "max_slope_deg",
    "max_step_m",
    "max_water_depth_m",
    "target_speed_mps",
    "terrain_elevation_m",
    "terrain_slope_deg",
    "terrain_roughness_m",
    "obstacle_height_m",
    "bearing_strength_pa",
    "friction_coefficient",
    "water_depth_m",
    "structural_capacity_n",
    "data_confidence",
]


def stack(features: dict[str, tf.Tensor], label: tf.Tensor) -> tuple[tf.Tensor, tf.Tensor]:
    values = tf.stack([tf.cast(features[name], tf.float32) for name in FEATURES], axis=-1)
    return values, tf.cast(label, tf.int32)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("csv", type=Path)
    parser.add_argument("--output", type=Path, default=Path("traversability.keras"))
    parser.add_argument("--epochs", type=int, default=30)
    args = parser.parse_args()

    dataset = tf.data.experimental.make_csv_dataset(
        str(args.csv),
        batch_size=32,
        label_name="label",
        select_columns=[*FEATURES, "label"],
        num_epochs=1,
        shuffle=True,
        shuffle_seed=1729,
    ).map(stack, num_parallel_calls=tf.data.AUTOTUNE)
    dataset = dataset.cache()
    validation = dataset.enumerate().filter(lambda index, _: index % 5 == 0).map(lambda _, row: row)
    training = dataset.enumerate().filter(lambda index, _: index % 5 != 0).map(lambda _, row: row)

    normalizer = tf.keras.layers.Normalization()
    normalizer.adapt(training.map(lambda features, _: features))
    model = tf.keras.Sequential(
        [
            tf.keras.Input(shape=(len(FEATURES),)),
            normalizer,
            tf.keras.layers.Dense(64, activation="relu"),
            tf.keras.layers.Dropout(0.15),
            tf.keras.layers.Dense(32, activation="relu"),
            tf.keras.layers.Dense(3, activation="softmax"),
        ]
    )
    model.compile(
        optimizer="adam",
        loss="sparse_categorical_crossentropy",
        metrics=["accuracy"],
    )
    model.fit(training.prefetch(tf.data.AUTOTUNE), validation_data=validation, epochs=args.epochs)
    model.save(args.output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
