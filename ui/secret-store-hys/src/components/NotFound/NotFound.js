import { Link } from "react-router-dom";
import "./NotFound.css";

export default function NotFound() {
  return (
    <div className="flex flex-column">
      <div className="flex align-items-center justify-content-center h-4rem font-bold m-2">
        <h1>404 - Not Found</h1>
      </div>
      <div className="flex align-items-center justify-content-center h-4rem m-2">
        <Link className="p-button  p-button-text" id="home-link" to="/">
          Home
        </Link>
      </div>
    </div>
  );
}
