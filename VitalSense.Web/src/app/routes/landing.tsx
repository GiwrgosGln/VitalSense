import Contact from "@/features/landing/components/contact";
import Features from "@/features/landing/components/features";
import Footer from "@/features/landing/components/footer";
import Hero from "@/features/landing/components/hero";
import Navbar from "@/features/landing/components/navbar";
import { Zap } from "lucide-react";

const LandingRoute = () => {
  return (
    <div className="flex flex-col min-h-screen max-screen overflow-hidden">
      <Navbar />
      <Hero
        heading="VitalSense: Health Monitoring Made Simple"
        description="Track, analyze, and improve your clients' health metrics with our comprehensive health monitoring platform."
        button={{
          text: "Get Started",
          url: "/auth/register",
          icon: <Zap className="ml-2 size-4" />,
        }}
        imageSrc="https://developers.elementor.com/docs/assets/img/elementor-placeholder-image.png"
        imageAlt="Vital Sense Dashboard"
      />
      <Features />
      <Contact />
      <Footer />
    </div>
  );
};

export default LandingRoute;
