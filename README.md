# FICSIT Assembly Planner

A high-performance, graph-based factory production planner for *Satisfactory*. 

The FICSIT Assembly Planner calculates exact machine counts, power requirements, and logistical routing for any target item. It visualizes the production line as a clean, interactive Directed Acyclic Graph (DAG), automatically compressing bypassed nodes and calculating bridging routes.

## Features

* **Interactive Graph Visualization:** Built on `reactflow`, providing a drag-and-drop, zoomable canvas of your factory layout.
* **Smart Auto-Layout:** Integrates `dagre` to automatically structure complex production chains from left to right, minimizing visual clutter.
* **Alternate Recipe Support:** Toggle unlocked alternate recipes in real-time to recalculate optimal production paths.
* **Yield & Power Analytics:** Hover over any node to see exact input/output ratios, machine counts, and MW power consumption.
* **Graph Compression:** Automatically identifies and bridges intermediate items to keep the visual diagram clean and focused on actual machine setup.

## Tech Stack

* **Frontend Framework:** React 18
* **Language:** TypeScript
* **Styling:** Tailwind CSS
* **Graph Rendering:** React Flow
* **Graph Math & Layout:** Dagre
* **Backend API:** .NET / C# (Expected at `https://localhost:7299`)

## Architecture

This application is built with strict separation of concerns to maintain a highly scalable codebase:

* **State Management:** Custom React hooks (`useFactoryGraph`, `useFactoryData`) handle all API interactions and local state.
* **Pure Utility Logic:** Heavy graph mathematics (compression, bridging, node mapping) are abstracted into pure functions (`graphParser.ts`) to keep components lightweight.
* **Modular UI:** The interface is broken down into purpose-built, single-responsibility components, making feature additions (like new sidebar controls) trivial.
