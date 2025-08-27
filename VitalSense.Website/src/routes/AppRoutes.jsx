import { Route, Routes } from "react-router-dom";
import { Home } from "@pages";
import { I18nextProvider } from "react-i18next";
import i18n from "../i18n";

export const AppRoutes = () => {
  return (
    <I18nextProvider i18n={i18n}>
      <Routes>
        <Route path="/" element={<Home />} />
      </Routes>
    </I18nextProvider>
  );
};
