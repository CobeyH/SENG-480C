import math

radii = [5.4, 6.0589, 6.7982, 7.6277, 8.5584, 9.6027, 10.7744, 12.0891, 13.5642, 15.2193, 17.0763]
rad2deg = 57.2958

#Given the radius of a cylinder, compute the arc length between braille dots within cells, and between adjacent cells.
for i in range(0,11):
    r = radii[i]
    arc_length_short = round(6.8 / r / math.pi * 180, 4)
    arc_length_half_short = round(6.8 / r / math.pi * 90, 4)
    arc_length_long = round(9.25 / r / math.pi * 180, 4)
    arc_length_half_long = round(9.25 / r / math.pi * 90, 4)
    arc_length_long_and_half_short = round(arc_length_half_short + arc_length_long, 4)
    print("Magnitude 5.", i)
    print("\t\tradius: ", r, ", arc_length_short: ", arc_length_short, ", arc_length_long: ", arc_length_long)
    print("\t\thalf_short:", arc_length_half_short, ", half_long: ", arc_length_half_long)
    print("\t\thalf_short_&_long: ", arc_length_long_and_half_short)