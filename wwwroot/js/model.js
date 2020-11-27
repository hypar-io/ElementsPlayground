import * as THREE from 'https://unpkg.com/three@0.123.0/build/three.module.js'

window.model = {
    loadScene: () => { loadScene(); },
};

function loadScene() {

    console.debug("Loading scene")
    const scene = new THREE.Scene();
    const div = document.getElementById("model");
    const camera = new THREE.PerspectiveCamera(75, div.clientWidth / div.clientHeight, 0.1, 1000);

    const renderer = new THREE.WebGLRenderer();

    renderer.setSize(div.clientWidth, div.clientHeight);
    div.appendChild(renderer.domElement);

    const geometry = new THREE.BoxGeometry();
    const material = new THREE.MeshBasicMaterial({ color: 0x00ff00 });
    const cube = new THREE.Mesh(geometry, material);
    scene.add(cube);

    camera.position.z = 5;

    const animate = function () {
        requestAnimationFrame(animate);

        cube.rotation.x += 0.01;
        cube.rotation.y += 0.01;

        renderer.render(scene, camera);
    };

    animate();
}