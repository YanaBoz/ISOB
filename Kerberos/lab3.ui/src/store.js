import { create } from "zustand";

const initialTGT = localStorage.getItem("tgt")
    ? JSON.parse(localStorage.getItem("tgt"))
    : null;
const initialST = localStorage.getItem("st")
    ? JSON.parse(localStorage.getItem("st"))
    : null;

export const useAuthStore = create((set) => ({
    tgt: initialTGT,
    st: initialST,
    setTGT: (tgt) => {
        console.log("Saving TGT:", tgt);
        localStorage.setItem("tgt", JSON.stringify(tgt));
        set({ tgt });
    },
    setST: (st) => {
        console.log("Saving ST:", st);
        if (st) {
            localStorage.setItem("st", JSON.stringify(st));
        } else {
            localStorage.removeItem("st");
        }
        set({ st });
    },
    logout: () => {
        localStorage.removeItem("tgt");
        localStorage.removeItem("st");
        set({ tgt: null, st: null });
    },
}));
