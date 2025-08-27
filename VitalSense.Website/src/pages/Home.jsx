import { Hero, Navbar } from "@components";

export default function Home() {
  return (
    <div className="bg-gradient-to-br from-purple-50 to-blue-100">
      <Navbar />
      <Hero />
    </div>
  );
}
