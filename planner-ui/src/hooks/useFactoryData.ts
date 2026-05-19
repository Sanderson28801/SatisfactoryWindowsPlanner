import { useState, useEffect } from "react";
import { type MetaItem } from "../types"; // Adjust path as needed

const API_BASE = "https://localhost:7299/api/factory";

export function useFactoryData() {
  const [items, setItems] = useState<MetaItem[]>([]);
  const [alternates, setAlternates] = useState<MetaItem[]>([]);

  useEffect(() => {
    fetch(`${API_BASE}/items`)
      .then((res) => res.json())
      .then(setItems)
      .catch(console.error);

    fetch(`${API_BASE}/alternates`)
      .then((res) => res.json())
      .then(setAlternates)
      .catch(console.error);
  }, []);

  return { items, alternates };
}
