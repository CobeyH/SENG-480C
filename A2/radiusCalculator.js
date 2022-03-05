const energies = [1.995262e12, 2.818383e12, 3.981072e12, 5.623413e12, 7.943282e12, 1.122018e13, 1.584893e13, 2.238721e13, 3.162278e13, 4.466836e13, 6.309573e13]

function computeVolume(r) {
    // PI * r^2 * h
    // h = 2r
    return Math.PI * r * r * (2 * r)
}

function computeRadius(v) {
    return Math.cbrt(v / (2 * Math.PI));
}

let scaleFactors = [];
let radii = [1.6]
let lastVolume = computeVolume(radii[0]);

for(let i = 1; i < energies.length; i++) {
 scaleFactors.push(energies[i]/energies[i-1]);
}

for(let i = 0; i < scaleFactors.length; i++) {
    lastVolume *= scaleFactors[i]
    radii.push(computeRadius(lastVolume));
}

for(let i = 0; i < 11; i++) {
    console.log("Magnitude: ", 5 + 0.1*i, " Radius: ", radii[i])
}
