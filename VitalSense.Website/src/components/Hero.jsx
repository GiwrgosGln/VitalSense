export default function Hero() {
  return (
    <section className="relative h-[80vh] pt-28 pb-0 overflow-hidden flex items-center">
      <div className="container mx-auto px-4 h-full flex flex-col justify-between">
        {/* Hero Content */}
        <div className="text-center pt-4 md:pt-8">
          <span className="inline-block py-1 px-3 rounded-full bg-white text-gray-700 text-sm mb-4">
            Lorem ipsum dolor sit amet consectetur adipiscing elit.
          </span>
          <h1 className="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
            Lorem ipsum dolor sit amet consectetur adipiscing elit.
          </h1>
          <p className="text-lg text-gray-700 max-w-2xl mx-auto">
            Lorem ipsum dolor sit amet consectetur adipiscing elit. Dolor sit
            amet consectetur adipiscing elit quisque faucibus.
          </p>
        </div>

        <div className="dashboard-container overflow-hidden mt-8">
          <img
            src="https://themeselection.com/wp-content/uploads/2025/06/14-slash-admin.png"
            alt="App Screenshot"
            className="rounded-t-lg shadow-xl w-full object-cover object-top h-[540px]"
          />
        </div>
      </div>
    </section>
  );
}
