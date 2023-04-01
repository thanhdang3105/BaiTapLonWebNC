// Import the functions you need from the SDKs you need
import { initializeApp } from "https://www.gstatic.com/firebasejs/9.18.0/firebase-app.js";
import { getStorage, ref, uploadBytes, getDownloadURL } from "https://www.gstatic.com/firebasejs/9.18.0/firebase-storage.js"
// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries

// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const firebaseConfig = {
    apiKey: "AIzaSyB_iqpa9W7YvCNRMr1526SSHBgR7iwHIyY",
    authDomain: "messenger-359a9.firebaseapp.com",
    projectId: "messenger-359a9",
    storageBucket: "messenger-359a9.appspot.com",
    messagingSenderId: "734254494044",
    appId: "1:734254494044:web:709cce883e06fc69905199",
    measurementId: "G-KXLK2LGK51"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);


window.firebaseUploadFile = async (file, name) => {
    const prefixType = file?.type?.split('/')?.[1];
    if (name.includes('.' + prefixType)){
        console.log(name)
    } else if (name) {
        name = name.replaceAll(' ', '_') + '.' + prefixType
    }
    const storage = getStorage();
    const storageRef = ref(storage, "WebNC/"+name);
    const res = await uploadBytes(storageRef, file);
    const imgSrc = await getDownloadURL(res.ref);
    return imgSrc
}