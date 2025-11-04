import React from "react";
import { useParams } from "react-router-dom";

export default function WheelSessionPage() {
    const { id } = useParams();
    return <h1>Wheel — session {id}</h1>;
}
