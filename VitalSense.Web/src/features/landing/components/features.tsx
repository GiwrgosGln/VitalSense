"use client";

import { useState } from "react";

import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";

interface FeatureItem {
  id: number;
  title: string;
  image: string;
  description: string;
}

interface FeatureProps {
  features?: FeatureItem[];
}

const defaultFeatures: FeatureItem[] = [
  {
    id: 1,
    title: "Google Calendar Integration",
    image:
      "https://developers.elementor.com/docs/assets/img/elementor-placeholder-image.png",
    description:
      "Seamlessly sync appointments between VitalSense and Google Calendar. Never double-book again and manage your entire schedule from one place. ",
  },
  {
    id: 2,
    title: "Comprehensive Task Management",
    image:
      "https://developers.elementor.com/docs/assets/img/elementor-placeholder-image.png",
    description:
      "Stay organized with our robust task management system. Create follow-ups, set reminders for client check-ins, and prioritize daily activities. ",
  },
  {
    id: 3,
    title: "Custom Meal Plan Templates",
    image:
      "https://developers.elementor.com/docs/assets/img/elementor-placeholder-image.png",
    description:
      "Create, save, and reuse personalized meal plan templates tailored to different dietary needs. Quickly adjust macros, calories, and food preferences for each client.",
  },
  {
    id: 4,
    title: "Interactive Questionnaires",
    image:
      "https://developers.elementor.com/docs/assets/img/elementor-placeholder-image.png",
    description:
      "Build custom questionnaires for client intake, progress tracking, and satisfaction surveys. Gather critical health information, food preferences, and lifestyle details to create truly personalized nutrition plans that drive better outcomes.",
  },
  {
    id: 5,
    title: "Seamless Client Import",
    image:
      "https://developers.elementor.com/docs/assets/img/elementor-placeholder-image.png",
    description:
      "Already have an established practice? Import your existing client database with our easy-to-use migration tools. ",
  },
];

const Features = ({ features = defaultFeatures }: FeatureProps) => {
  const [activeTabId, setActiveTabId] = useState<number | null>(1);
  const [activeImage, setActiveImage] = useState(features[0].image);

  return (
    <section id="features" className="py-32">
      <div className="container mx-auto">
        <div className="mb-12 flex w-full items-start justify-between gap-12">
          <div className="w-full md:w-1/2">
            <Accordion type="single" className="w-full" defaultValue="item-1">
              {features.map((tab) => (
                <AccordionItem key={tab.id} value={`item-${tab.id}`}>
                  <AccordionTrigger
                    onClick={() => {
                      setActiveImage(tab.image);
                      setActiveTabId(tab.id);
                    }}
                    className="cursor-pointer py-5 no-underline! transition"
                  >
                    <h6
                      className={`text-xl font-semibold ${
                        tab.id === activeTabId
                          ? "text-foreground"
                          : "text-muted-foreground"
                      }`}
                    >
                      {tab.title}
                    </h6>
                  </AccordionTrigger>
                  <AccordionContent>
                    <p className="mt-3 text-muted-foreground">
                      {tab.description}
                    </p>
                    <div className="mt-4 md:hidden">
                      <img
                        src={tab.image}
                        alt={tab.title}
                        className="h-full max-h-80 w-full rounded-md object-cover"
                      />
                    </div>
                  </AccordionContent>
                </AccordionItem>
              ))}
            </Accordion>
          </div>
          <div className="relative m-auto hidden w-1/2 overflow-hidden rounded-xl bg-muted md:block">
            <img
              src={activeImage}
              alt="Feature preview"
              className="aspect-4/3 rounded-md object-cover pl-4"
            />
          </div>
        </div>
      </div>
    </section>
  );
};

export default Features;
