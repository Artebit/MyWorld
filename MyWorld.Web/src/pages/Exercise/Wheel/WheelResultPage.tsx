import React from "react";
import { useParams } from "react-router-dom";

export default function WheelResultPage() {
    const { id } = useParams();
    return <h1>Wheel — result {id}</h1>;
}
