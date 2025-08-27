import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

export default function Navbar() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const { t } = useTranslation();

  return (
    <nav className="bg-transparent py-4 px-6 absolute top-0 left-0 right-0 z-50">
      <div className="container mx-auto flex items-center justify-between">
        {/* Logo */}
        <Link to="/" className="flex items-center space-x-2">
          <svg
            className="w-8 h-8 text-purple-600"
            viewBox="0 0 24 24"
            fill="currentColor"
          >
            <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5" />
          </svg>
          <span className="text-xl font-bold text-purple-600">
            {t("navbar.title")}
          </span>
        </Link>

        {/* Desktop Navigation */}
        <div className="hidden md:flex items-center space-x-8">
          <Link
            to="/"
            className="font-medium text-gray-600 hover:text-purple-600 transition-colors"
          >
            {t("navbar.home")}
          </Link>
          <Link
            to="/pricing"
            className="font-medium text-gray-600 hover:text-purple-600 transition-colors"
          >
            {t("navbar.pricing")}
          </Link>
          <Link
            to="/faq"
            className="font-medium text-gray-600 hover:text-purple-600 transition-colors"
          >
            {t("navbar.faq")}
          </Link>
          <Link
            to="/contact"
            className="font-medium text-gray-600 hover:text-purple-600 transition-colors"
          >
            {t("navbar.contact")}
          </Link>
        </div>

        {/* Login Button */}
        <div className="hidden md:block">
          <Link
            to="https://vitalsense.gr/login"
            className="bg-gray-900 text-white px-4 py-2 rounded-full font-medium flex items-center"
          >
            {t("navbar.login")}
            <svg
              className="w-5 h-5 ml-2"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M14 5l7 7m0 0l-7 7m7-7H3"
              />
            </svg>
          </Link>
        </div>

        {/* Mobile Menu Button */}
        <button
          className="md:hidden text-purple-600 focus:outline-none z-[60]"
          onClick={() => setIsMenuOpen(!isMenuOpen)}
        >
          {isMenuOpen ? (
            <svg
              className="w-7 h-7"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          ) : (
            <svg
              className="w-7 h-7"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M4 6h16M4 12h16m-7 6h7"
              />
            </svg>
          )}
        </button>
      </div>

      {/* Mobile Menu */}
      {isMenuOpen && (
        <div className="md:hidden fixed inset-0 bg-gradient-to-br from-purple-50 to-blue-100 z-50 pt-20 px-6">
          <div className="flex flex-col space-y-6">
            <Link
              to="/"
              className="font-medium text-gray-600 hover:text-purple-600 transition-colors text-xl"
              onClick={() => setIsMenuOpen(false)}
            >
              {t("navbar.home")}
            </Link>
            <Link
              to="/pricing"
              className="font-medium text-gray-600 hover:text-purple-600 transition-colors text-xl"
              onClick={() => setIsMenuOpen(false)}
            >
              {t("navbar.pricing")}
            </Link>
            <Link
              to="/faq"
              className="font-medium text-gray-600 hover:text-purple-600 transition-colors text-xl"
              onClick={() => setIsMenuOpen(false)}
            >
              {t("navbar.faq")}
            </Link>
            <Link
              to="/contact"
              className="font-medium text-gray-600 hover:text-purple-600 transition-colors text-xl"
              onClick={() => setIsMenuOpen(false)}
            >
              {t("navbar.contact")}
            </Link>
            <Link
              to="https://vitalsense.gr/login"
              className="bg-gray-900 text-white px-6 py-3 rounded-full font-medium flex items-center w-fit mt-4"
              onClick={() => setIsMenuOpen(false)}
            >
              {t("navbar.login")}
              <svg
                className="w-5 h-5 ml-2"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M14 5l7 7m0 0l-7 7m7-7H3"
                />
              </svg>
            </Link>
          </div>
        </div>
      )}
    </nav>
  );
}
