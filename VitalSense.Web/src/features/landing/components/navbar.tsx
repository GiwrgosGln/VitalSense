import { MenuIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  NavigationMenu,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
  navigationMenuTriggerStyle,
} from "@/components/ui/navigation-menu";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import logo from "@/assets/logo.png";
import { useNavigate } from "react-router-dom";

const Navbar = () => {
  const navigate = useNavigate();

  const scrollToSection = (id: string) => {
    const element = document.getElementById(id);
    if (element) {
      element.scrollIntoView({
        behavior: "smooth",
      });
    }
  };

  return (
    <section className="py-4">
      <div className="w-screen px-2 md:px-10">
        <nav className="flex items-center justify-between">
          <a
            href="https://vital-sense.vercel.app/"
            className="flex items-center gap-2"
          >
            <img src={logo} className="max-h-8" alt="Vital Sense" />
            <span className="text-lg font-semibold tracking-tighter">
              VitalSense
            </span>
          </a>
          <NavigationMenu className="hidden lg:block">
            <NavigationMenuList>
              <NavigationMenuItem>
                <NavigationMenuLink
                  onClick={(e) => {
                    e.preventDefault();
                    scrollToSection("features");
                  }}
                  href="#features"
                  className={navigationMenuTriggerStyle()}
                >
                  Features
                </NavigationMenuLink>
              </NavigationMenuItem>
              <NavigationMenuItem>
                <NavigationMenuLink
                  onClick={(e) => {
                    e.preventDefault();
                    scrollToSection("contact");
                  }}
                  href="#contact"
                  className={navigationMenuTriggerStyle()}
                >
                  Contact
                </NavigationMenuLink>
              </NavigationMenuItem>
            </NavigationMenuList>
          </NavigationMenu>
          <div className="hidden items-center gap-4 lg:flex">
            <Button onClick={() => navigate("/auth/login")}>Sign in</Button>
          </div>
          <Sheet>
            <SheetTrigger asChild className="lg:hidden">
              <Button variant="outline" size="icon">
                <MenuIcon className="h-4 w-4" />
              </Button>
            </SheetTrigger>
            <SheetContent side="top" className="max-h-screen overflow-auto">
              <SheetHeader>
                <SheetTitle>
                  <a
                    href="https://vital-sense.vercel.app/"
                    className="flex items-center gap-2"
                  >
                    <img src={logo} className="max-h-8" alt="Vital Sense" />
                    <span className="text-lg font-semibold tracking-tighter">
                      VitalSense
                    </span>
                  </a>
                </SheetTitle>
              </SheetHeader>
              <div className="flex flex-col p-4">
                <div className="flex flex-col gap-6">
                  <a
                    href="#features"
                    className="font-medium"
                    onClick={(e) => {
                      e.preventDefault();
                      scrollToSection("features");
                      document
                        .querySelector('[data-state="open"]')
                        ?.setAttribute("data-state", "closed");
                    }}
                  >
                    Features
                  </a>
                  <a
                    href="#contact"
                    className="font-medium"
                    onClick={(e) => {
                      e.preventDefault();
                      scrollToSection("contact");
                      document
                        .querySelector('[data-state="open"]')
                        ?.setAttribute("data-state", "closed");
                    }}
                  >
                    Contact
                  </a>
                </div>
                <div className="mt-6 flex flex-col gap-4">
                  <Button onClick={() => navigate("/auth/login")}>
                    Sign in
                  </Button>
                </div>
              </div>
            </SheetContent>
          </Sheet>
        </nav>
      </div>
    </section>
  );
};

export default Navbar;
